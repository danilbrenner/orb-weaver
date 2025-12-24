package infra

import cats.effect.{IO, Resource}
import org.apache.kafka.clients.consumer.{ConsumerConfig, ConsumerRecords, KafkaConsumer}
import org.apache.kafka.common.serialization.StringDeserializer

import java.time.Duration
import java.util.Properties
import scala.jdk.CollectionConverters._

class KafkaListener(config: Map[String, String]) {

  private def props: Properties = {
    val p = new Properties()
    p.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, classOf[StringDeserializer].getName)
    p.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, classOf[StringDeserializer].getName)
    p.put(ConsumerConfig.ENABLE_AUTO_COMMIT_CONFIG, "false")
    p.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest")
    config.foreach { case (k, v) => p.put(k, v) }
    p
  }

  private def consumerResource: Resource[IO, KafkaConsumer[String, String]] =
    Resource.make {
      IO.blocking(new KafkaConsumer[String, String](props))
    } { c =>
      IO.blocking(c.close()).handleErrorWith(_ => IO.unit)
    }

  private def runLoop(c: KafkaConsumer[String, String], topic: String, process: (String, String) => IO[Unit]): IO[Unit] = {
    val subscribe: IO[Unit] = IO.blocking(c.subscribe(java.util.List.of(topic)))

    def onePoll: IO[Unit] =
      IO.blocking(c.poll(Duration.ofMillis(500))).flatMap { (records: ConsumerRecords[String, String]) =>
        val batch = records.iterator().asScala.toList

        val processBatch =
          batch.foldLeft(IO.unit) { (acc, r) =>
            acc *> process(
              Option(r.key()).getOrElse(""),
              Option(r.value()).getOrElse("")
            )
          }

        processBatch *> IO.blocking(c.commitSync())
      }

    val loop =
      onePoll.foreverM.handleErrorWith { IO.raiseError }

    subscribe *> loop.onCancel(IO.blocking(c.wakeup()).void)
  }

  def start(topic: String, process: (String, String) => IO[Unit]): IO[Unit] = {
    consumerResource.use { c =>
      runLoop(c, topic, process)
    }
  }
}

object KafkaListener {
  def apply(config: Map[String, String]): KafkaListener =
    new KafkaListener(config)
}