import cats.effect.{IO, Resource}
import cats.implicits.*
import org.typelevel.log4cats.slf4j.Slf4jLogger
import org.apache.kafka.clients.consumer.{ConsumerConfig, KafkaConsumer}
import org.apache.kafka.common.serialization.StringDeserializer

import scala.jdk.CollectionConverters.*
import java.time.Duration
import java.util.Properties

object KafkaSubscriber {
  private val logger = Slf4jLogger.getLogger[IO]

  def subscribe(topic: String, bootstrap: String): IO[Unit] = {
    val props = new Properties()
    props.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, bootstrap)
    props.put(ConsumerConfig.GROUP_ID_CONFIG, "orb-weaver-group")
    props.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, classOf[StringDeserializer].getName)
    props.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, classOf[StringDeserializer].getName)
    props.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest")

    val consumerResource: Resource[IO, KafkaConsumer[String, String]] =
      Resource.make(IO.blocking(new KafkaConsumer[String, String](props)))(c => IO.blocking(c.close()))

    consumerResource.use { consumer =>
      for {
        _ <- IO.blocking(consumer.subscribe(java.util.List.of(topic)))
        _ <- logger.info(s"Subscribed to topic '$topic' (bootstrap=$bootstrap)")
        _ <- pollLoop(consumer)
      } yield ()
    }
  }

  private def pollLoop(consumer: KafkaConsumer[String, String]): IO[Unit] = {
    IO.blocking(consumer.poll(Duration.ofMillis(500))).flatMap { records =>
      val recs = records.asScala.toList
      recs.traverse_ { r =>
        logger.info(s"Consumed record: topic=${r.topic()} partition=${r.partition()} offset=${r.offset()} key=${r.key()} value=${r.value()}")
      } *> pollLoop(consumer)
    }
  }
}

