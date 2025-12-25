package data

import domain.Message
import doobie.ConnectionIO
import doobie.implicits._

trait MessageRepository {
  def insertMessage(message: Message): ConnectionIO[Int]
}

class MessageRepositoryImpl extends MessageRepository {
  def insertMessage(message: Message): ConnectionIO[Int] =
    sql"""insert into messages_log (message, hash) values (${message.content}, ${message.hash})
          on conflict (hash) do nothing"""
      .update
      .run
}

object MessageRepository {
  def apply(): MessageRepository = new MessageRepositoryImpl()
}

