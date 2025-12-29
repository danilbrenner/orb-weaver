ThisBuild / version := "0.1.0-SNAPSHOT"

ThisBuild / scalaVersion := "3.3.7"

lazy val root = (project in file("."))
  .settings(
    name := "orb-weaver"
  )

val DoobieVersion = "1.0.0-RC11"

libraryDependencies ++= Seq(
  "org.tpolecat" %% "doobie-core" % DoobieVersion,
  "org.tpolecat" %% "doobie-postgres" % DoobieVersion,
  "org.tpolecat" %% "doobie-hikari" % DoobieVersion,
  "org.tpolecat" %% "doobie-postgres-circe" % DoobieVersion,
  "org.typelevel" %% "cats-effect" % "3.6.3",
  "org.apache.kafka" % "kafka-clients" % "4.1.1",
  "org.typelevel" %% "log4cats-slf4j" % "2.7.1",
  "org.slf4j" % "slf4j-api" % "2.0.17",
  "ch.qos.logback" % "logback-classic" % "1.5.23",
  "net.logstash.logback" % "logstash-logback-encoder" % "9.0",
  "com.jayway.jsonpath" % "json-path" % "2.10.0",
  "org.scalatest" %% "scalatest" % "3.2.19" % Test
)