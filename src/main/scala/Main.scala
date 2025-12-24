import cats.effect.{IO, IOApp}
import infra.KafkaListener
import org.apache.kafka.clients.consumer.ConsumerConfig
import org.typelevel.log4cats.StructuredLogger
import org.typelevel.log4cats.slf4j.Slf4jFactory

object Main extends IOApp.Simple {
  private def processMessage(logger: StructuredLogger[IO])(key: String, value: String): IO[Unit] = {
    logger.info(Map("message" -> value))("Processing message")
  }

  override def run: IO[Unit] = {

    val loggerFactory = Slf4jFactory.create[IO]
    val baseLogger: StructuredLogger[IO] = loggerFactory.getLogger

    KafkaListener(Map(
      ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG -> "localhost:9092",
      ConsumerConfig.GROUP_ID_CONFIG -> "orb-weaver-group",
      ConsumerConfig.MAX_POLL_RECORDS_CONFIG -> "10")
    )
      .start(
        "probe_outcomes",
        processMessage(
          StructuredLogger.withContext(baseLogger)(Map("topic" -> "probe_outcomes"))
        )
      )
      .handleErrorWith(t => baseLogger.error(t)(s"Kafka listener failed: ${t.getMessage}"))
  }
}