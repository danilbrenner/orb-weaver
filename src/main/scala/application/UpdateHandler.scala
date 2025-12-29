package application

import cats.effect.IO
import domain.AlertUpdate
import commons.Logging.*
import data.MessageLogRepository
import doobie.Transactor
import doobie.implicits.toConnectionIOOps
import org.typelevel.log4cats.StructuredLogger

class UpdateHandler(l: StructuredLogger[IO], repository: MessageLogRepository) {
  def handle (tx: Transactor[IO]) (msg: AlertUpdate): IO[Unit] = {
    l.withContext(Map("message_hash" -> msg.hash)) { logger =>
      for {
        _ <- logger.info(s"Received message: ${msg.content}")
        insertedRows <- repository.insertMessage(msg).transact(tx)
        _ <- if (insertedRows > 0) {
          logger.info(s"Inserted message with hash: ${msg.hash}")
        } else
          logger.warn(s"Duplicate message skipped")
      } yield ()
    }
  }
} 
