package application

import cats.effect.IO
import domain.MessageDocument
import commons.Logging.*
import data.MessageRepository
import doobie.Transactor
import doobie.implicits.toConnectionIOOps
import org.typelevel.log4cats.StructuredLogger

class MessageHandler(l: StructuredLogger[IO], repository: MessageRepository) {
  def handle (tx: Transactor[IO]) (msg: MessageDocument): IO[Unit] = {
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
