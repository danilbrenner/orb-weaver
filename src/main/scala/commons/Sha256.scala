package commons

import java.nio.charset.StandardCharsets
import java.security.MessageDigest

object Sha256 {

  private def digestBytes(bytes: Array[Byte]): Array[Byte] = {
    val md = MessageDigest.getInstance("SHA-256")
    md.digest(bytes)
  }

  private def digestHex(bytes: Array[Byte]): String = {
    val hash = digestBytes(bytes)
    val sb = new StringBuilder(hash.length * 2)
    hash.foreach { b => sb.append(f"${b & 0xff}%02x") }
    sb.result()
  }

  def digestHexUtf8(s: String): String =
    digestHex(s.getBytes(StandardCharsets.UTF_8))
}
