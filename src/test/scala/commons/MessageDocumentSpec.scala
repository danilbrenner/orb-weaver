package commons

import domain.MessageDocument
import org.scalatest.matchers.should.Matchers
import org.scalatest.wordspec.AnyWordSpec

class MessageDocumentSpec extends AnyWordSpec with Matchers {
  "MessageDocument.apply" should {
    "create a MessageDocument from valid JSON string" in {
      val json = """{"name": "John", "age": 30}"""
      val doc = MessageDocument(json)

      doc shouldBe a[MessageDocument]
    }

    "handle empty JSON object" in {
      val json = """{}"""
      val doc = MessageDocument(json)

      doc shouldBe a[MessageDocument]
    }

    "handle empty string" in {
      val json = ""
      val doc = MessageDocument(json)

      doc shouldBe a[MessageDocument]
    }

    "handle space string" in {
      val json = " "
      val doc = MessageDocument(json)

      doc shouldBe a[MessageDocument]
    }

    "handle string" in {
      val json = "Hi there!"
      val doc = MessageDocument(json)

      doc shouldBe a[MessageDocument]
    }
  }
  "MessageDocument.getField" should {
    "read root string field" in {
      val json = "Hi there!"
      val doc = MessageDocument(json)

      doc.getFieldValue("$") shouldBe Some("Hi there!")
    }

    "read simple fields" in {
      val json = """{"name": "Alice", "age": 25}"""
      val doc = MessageDocument(json)

      doc.getFieldValue("$.name") shouldBe Some("Alice")
      doc.getFieldValue("$.age") shouldBe Some("25")
    }

    "read nested fields" in {
      val json = """{"person": {"name": "Bob", "age": 35}}"""
      val doc = MessageDocument(json)

      doc.getFieldValue("$.person.name") shouldBe Some("Bob")
      doc.getFieldValue("$.person.age") shouldBe Some("35")
    }

    "read array elements" in {
      val json = """{"items": ["apple", "banana", "cherry"]}"""
      val doc = MessageDocument(json)

      doc.getFieldValue("$.items[0]") shouldBe Some("apple")
      doc.getFieldValue("$.items[1]") shouldBe Some("banana")
      doc.getFieldValue("$.items[2]") shouldBe Some("cherry")
    }

    "read complex nested structures" in {
      val json = """{"store": {"book": [{"title": "Book One", "price": 10.99}, {"title": "Book Two", "price": 15.99}]}}"""
      val doc = MessageDocument(json)

      doc.getFieldValue("$.store.book[0].title") shouldBe Some("Book One")
      doc.getFieldValue("$.store.book[0].price") shouldBe Some("10.99")
      doc.getFieldValue("$.store.book[1].title") shouldBe Some("Book Two")
    }

    "read boolean values" in {
      val json = """{"active": true, "verified": false}"""
      val doc = MessageDocument(json)

      doc.getFieldValue("$.active") shouldBe Some("true")
      doc.getFieldValue("$.verified") shouldBe Some("false")
    }

    "read string field when null value present" in {
      val json = """{"name": "Test", "value": null}"""
      val doc = MessageDocument(json)

      doc.getFieldValue("$.name") shouldBe Some("Test")
    }

    "return None for non-existent field" in {
      val json = """{"active": true, "verified": false}"""
      val doc = MessageDocument(json)

      doc.getFieldValue("$.notExisting") shouldBe None
    }

    "filter array by boolean field using index" in {
      val json = """{"users": [{"name": "Alice", "active": true}, {"name": "Bob", "active": false}, {"name": "Charlie", "active": true}]}"""
      val doc = MessageDocument(json)

      doc.getFieldValue("$.users[?(@.active == true)]") shouldBe Some("""[{"name":"Alice","active":true},{"name":"Charlie","active":true}]""")
    }

    "filter array by numeric field using index" in {
      val json = """{"products": [{"name": "Book", "price": 10}, {"name": "Pen", "price": 5}, {"name": "Notebook", "price": 15}]}"""
      val doc = MessageDocument(json)

      doc.getFieldValue("$.products[?(@.price > 10)]") shouldBe Some("""[{"name":"Notebook","price":15}]""")
    }

    "filter array by numeric field using index return empty" in {
      val json = """{"products": [{"name": "Book", "price": 10}, {"name": "Pen", "price": 5}, {"name": "Notebook", "price": 15}]}"""
      val doc = MessageDocument(json)

      doc.getFieldValue("$.products[?(@.price > 100)]") shouldBe Some("[]")
    }
  }
  "MessageDocument.evaluate" should {
    "return true for existing non-empty string field" in {
      val json = """{"name": "Alice", "age": 25}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.name") shouldBe true
    }

    "return true for existing numeric field" in {
      val json = """{"name": "Alice", "age": 25}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.age") shouldBe true
    }

    "return true for existing boolean true field" in {
      val json = """{"active": true}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.active") shouldBe true
    }

    "return true for existing boolean false field" in {
      val json = """{"active": false}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.active") shouldBe true
    }

    "return false for null field" in {
      val json = """{"name": "Test", "value": null}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.value") shouldBe false
    }

    "return false for non-existent field" in {
      val json = """{"name": "Alice"}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.age") shouldBe false
    }

    "return true for non-empty object" in {
      val json = """{"person": {"name": "Bob", "age": 35}}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.person") shouldBe true
    }

    "return false for empty object" in {
      val json = """{"person": {}}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.person") shouldBe false
    }

    "return true for non-empty array" in {
      val json = """{"items": ["apple", "banana", "cherry"]}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.items") shouldBe true
    }

    "return false for empty array" in {
      val json = """{"items": []}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.items") shouldBe false
    }

    "return true for filter matching elements" in {
      val json = """{"users": [{"name": "Alice", "active": true}, {"name": "Bob", "active": false}]}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.users[?(@.active == true)]") shouldBe true
    }

    "return false for filter matching no elements" in {
      val json = """{"users": [{"name": "Alice", "active": false}, {"name": "Bob", "active": false}]}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.users[?(@.active == true)]") shouldBe false
    }

    "return true for filter with numeric comparison" in {
      val json = """{"products": [{"name": "Book", "price": 10}, {"name": "Pen", "price": 5}, {"name": "Notebook", "price": 15}]}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.products[?(@.price > 10)]") shouldBe true
    }

    "return false for filter with numeric comparison matching nothing" in {
      val json = """{"products": [{"name": "Book", "price": 10}, {"name": "Pen", "price": 5}]}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.products[?(@.price > 100)]") shouldBe false
    }

    "return true for nested path with existing value" in {
      val json = """{"store": {"book": [{"title": "Book One", "price": 10.99}]}}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.store.book[0].title") shouldBe true
    }

    "return false for nested path with non-existent value" in {
      val json = """{"store": {"book": []}}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.store.book[0].title") shouldBe false
    }

    "return true for empty string field" in {
      val json = """{"name": ""}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.name") shouldBe true
    }

    "return true for zero numeric value" in {
      val json = """{"count": 0}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.count") shouldBe true
    }

    "return true for array element access" in {
      val json = """{"items": ["apple", "banana"]}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.items[0]") shouldBe true
    }

    "return false for out of bounds array access" in {
      val json = """{"items": ["apple", "banana"]}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.items[10]") shouldBe false
    }

    "return true for complex filter with AND condition" in {
      val json = """{"users": [{"name": "Alice", "age": 30, "active": true}, {"name": "Bob", "age": 25, "active": false}]}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.users[?(@.active == true && @.age >= 30)]") shouldBe true
    }

    "return false for complex filter with AND condition matching nothing" in {
      val json = """{"users": [{"name": "Alice", "age": 30, "active": true}, {"name": "Bob", "age": 25, "active": false}]}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.users[?(@.active == true && @.age > 50)]") shouldBe false
    }

    "return true for complex filter with OR condition" in {
      val json = """{"users": [{"name": "Alice", "role": "user"}, {"name": "Bob", "role": "admin"}]}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.users[?(@.role == 'admin' || @.role == 'moderator')]") shouldBe true
    }

    "return true for string with special characters" in {
      val json = """{"message": "Hello, World!"}"""
      val doc = MessageDocument(json)

      doc.evaluate("$.message") shouldBe true
    }
  }
}
