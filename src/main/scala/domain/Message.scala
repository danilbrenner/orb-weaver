package domain

import java.nio.charset.StandardCharsets
import java.security.MessageDigest

case class Message(content: String) {
  def hash: String = Sha256.digestHexUtf8(content)
}

object Message {
  def apply(content: String): Message = new Message(content)
}

object Sha256 {

  def digestBytes(bytes: Array[Byte]): Array[Byte] = {
    val md = MessageDigest.getInstance("SHA-256")
    md.digest(bytes)
  }

  def digestHex(bytes: Array[Byte]): String = {
    val hash = digestBytes(bytes)
    val sb = new StringBuilder(hash.length * 2)
    hash.foreach { b => sb.append(f"${b & 0xff}%02x") }
    sb.result()
  }

  def digestHexUtf8(s: String): String =
    digestHex(s.getBytes(StandardCharsets.UTF_8))
}
