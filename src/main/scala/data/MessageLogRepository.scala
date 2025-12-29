package data

import domain.AlertUpdate
import doobie.ConnectionIO
import doobie.implicits._

trait MessageLogRepository {
  def insertMessage(message: AlertUpdate): ConnectionIO[Int]
}

class MessageLogRepositoryImpl extends MessageLogRepository {
  def insertMessage(message: AlertUpdate): ConnectionIO[Int] =
    sql"""insert into messages_log (message, hash) values (${message.content}, ${message.hash})
          on conflict (hash) do nothing"""
      .update
      .run
}

object MessageLogRepository {
  def apply(): MessageLogRepository = new MessageLogRepositoryImpl()
}

