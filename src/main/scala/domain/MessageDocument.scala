package domain

import com.jayway.jsonpath.{DocumentContext, JsonPath, PathNotFoundException}
import commons.Sha256

import scala.util.chaining.scalaUtilChainingOps

trait MessageDocument {
  def hash: String
  
  def content: String

  def getFieldValue(jsonPath: String): Option[String]

  def evaluate(expression: String): Boolean
}

private class JsonMessageDocument(jsonDoc: DocumentContext) extends MessageDocument {

  override def getFieldValue(jsonPath: String): Option[String] = {
    try
      jsonDoc.read(jsonPath).toString.pipe(Some(_))
    catch {
      case _: PathNotFoundException => None
    }
  }

  override def evaluate(expression: String): Boolean = {
    try
      jsonDoc.read[Any](expression) match {
        case null => false
        case m: java.util.Map[_, _] => !m.isEmpty
        case l: java.util.List[_] => !l.isEmpty
        case a: Array[_] => !a.isEmpty
        case _ => true
      }
    catch {
      case _: PathNotFoundException => false
    }
  }
  
  override def content: String =
    jsonDoc.jsonString()
  
  override def hash: String =
    Sha256.digestHexUtf8(jsonDoc.jsonString())
}

object MessageDocument {
  def apply(json: String): MessageDocument =
    if (json.trim.isEmpty)
      JsonPath.parse("{}").pipe(new JsonMessageDocument(_))
    else
      JsonPath.parse(json).pipe(new JsonMessageDocument(_))
}