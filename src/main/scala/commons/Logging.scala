package commons

import cats.effect.IO
import org.typelevel.log4cats.StructuredLogger

import scala.util.chaining.scalaUtilChainingOps

object Logging:
  extension (logger: StructuredLogger[IO]) {
    def withContext(context: Map[String, String])(action: StructuredLogger[IO] => IO[Unit]): IO[Unit] =
      StructuredLogger.withContext(logger)(context).pipe(action)
  }
