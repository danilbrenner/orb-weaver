import application.UpdateHandler
import cats.effect.{IO, IOApp}
import domain.AlertUpdate
import doobie.Transactor
import infra.KafkaListener
import org.apache.kafka.clients.consumer.ConsumerConfig
import org.typelevel.log4cats.StructuredLogger
import org.typelevel.log4cats.slf4j.Slf4jFactory

import scala.util.chaining.scalaUtilChainingOps

object Main extends IOApp.Simple {

  override def run: IO[Unit] = {

    val loggerFactory = Slf4jFactory.create[IO]
    val baseLogger: StructuredLogger[IO] = loggerFactory.getLogger
    val repository = data.MessageLogRepository()

    val tx: Transactor[IO] =
      Transactor.fromDriverManager[IO](
        driver = "org.postgresql.Driver",
        url = "jdbc:postgresql://localhost:5435/weaver_db",
        user = "postgres",
        password = "postgres",
        logHandler = None
      )

    val handler = new UpdateHandler(
      StructuredLogger.withContext(baseLogger)(Map("topic" -> "probe_outcomes")),
      repository
    )

    KafkaListener(Map(
      ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG -> "localhost:9092",
      ConsumerConfig.GROUP_ID_CONFIG -> "orb-weaver-group",
      ConsumerConfig.MAX_POLL_RECORDS_CONFIG -> "1")
    )
      .start("probe_outcomes", (_, msg) => AlertUpdate(msg).pipe(handler.handle(tx)))
      .handleErrorWith(t => baseLogger.error(t)(s"Kafka listener failed: ${t.getMessage}"))
  }
}