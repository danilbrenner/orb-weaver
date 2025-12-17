import cats.effect.{IO, IOApp}
import org.typelevel.log4cats.slf4j.Slf4jLogger

object Main extends IOApp.Simple {
  private val logger = Slf4jLogger.getLogger[IO]
  
  def run: IO[Unit] = {
    val bootstrap = sys.env.getOrElse("KAFKA_BOOTSTRAP", "localhost:9092")
    val topic = sys.env.getOrElse("KAFKA_TOPIC", "probe_outcomes")

    for {
      _ <- logger.info("Starting Application...")
      _ <- KafkaSubscriber.subscribe(topic, bootstrap)
    } yield ()
  }
}