﻿/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements. See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership. The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License. You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing,
* software distributed under the License is distributed on an
* "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
* KIND, either express or implied. See the License for the
* specific language governing permissions and limitations
* under the License.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortCMIS.Binding.Browser.Json;
using System;
using System.IO;
using System.Numerics;

namespace PortCMISTests
{
    [TestClass]
    public class JsonTest
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void TestBasicJson()
        {
            JsonParser jp = new JsonParser();

            // ---
            string json1 = "{ \"test\":\"test-value\" }";
            JsonObject jo1 = (JsonObject)jp.Parse(new StringReader(json1));

            Assert.AreEqual("test-value", jo1["test"]);

            StringWriter writer = new StringWriter();
            jo1.WriteJsonString(writer);
            Assert.AreEqual("{\"test\":\"test-value\"}", writer.ToString());

            // ---
            string json2 = "{ \"s\":\"test\", \"i\":42, \"i0\":0, \"b\":true, \"d\":123.456, \"d0\": 0.0, \"n\":null}";
            JsonObject jo2 = (JsonObject)jp.Parse(new StringReader(json2));

            Assert.AreEqual("test", jo2["s"]);
            Assert.AreEqual(new BigInteger(42), jo2["i"]);
            Assert.AreEqual(BigInteger.Zero, jo2["i0"]);
            Assert.AreEqual(true, jo2["b"]);
            Assert.AreEqual(new Decimal(123.456), jo2["d"]);
            Assert.AreEqual(new Decimal(0), jo2["d0"]);
            Assert.IsNull(jo2["n"]);
        }

        [TestMethod]
        public void TestCreateJson()
        {
            JsonObject jo1 = new JsonObject();
            jo1.Add("s", "test");
            jo1.Add("i", 42);
            jo1.Add("i0", 0);
            jo1.Add("i1", new BigInteger(21));
            jo1.Add("b-true", true);
            jo1.Add("b-false", false);
            jo1.Add("d", new decimal(123.456));
            jo1.Add("d0", new decimal(0.0));
            jo1.Add("f", 98.76);
            jo1.Add("n", null);

            string jsonStr = jo1.ToString();
            TestContext.WriteLine("{0}", jsonStr);

            JsonParser jp = new JsonParser();
            JsonObject jo2 = (JsonObject)jp.Parse(new StringReader(jsonStr));

            Assert.AreEqual(jo1["s"], jo2["s"]);
            Assert.AreEqual(new BigInteger((int)jo1["i"]), jo2["i"]);
            Assert.AreEqual(new BigInteger((int)jo1["i0"]), jo2["i0"]);
            Assert.AreEqual(jo1["i1"], jo2["i1"]);
            Assert.AreEqual(jo1["b-true"], jo2["b-true"]);
            Assert.AreEqual(jo1["b-false"], jo2["b-false"]);
            Assert.AreEqual(jo1["d"], jo2["d"]);
            Assert.AreEqual(jo1["d0"], new decimal((int)((BigInteger)jo2["d0"])));
            Assert.AreEqual(new decimal((double)jo1["f"]), jo2["f"]);
            Assert.AreEqual(jo1["n"], jo2["n"]);
        }

        [TestMethod]
        public void TestArray()
        {
            JsonParser jp = new JsonParser();

            string json = "[ \"a\" , \"b\" , \"c\" , \"d\" , \"e\" , \"f\" , \"g\" , \"h\" , \"i\" ]";

            JsonArray ja1 = (JsonArray)jp.Parse(new StringReader(json));

            Assert.AreEqual(9, ja1.Count);
            for (char c = 'a'; c <= 'i'; c++)
            {
                Assert.AreEqual(c.ToString(), ja1[c - 'a']);
            }
        }

        [TestMethod]
        public void TestNestedJson()
        {
            JsonParser jp = new JsonParser();

            string json1 = "{ \"level1\":\"one\", \"nested\":{ \"level2\":\"two\"} }";
            JsonObject jo1 = (JsonObject)jp.Parse(new StringReader(json1));

            Assert.AreEqual("one", jo1["level1"]);
            Assert.AreEqual(2, jo1.Count);
            Assert.AreEqual("two", ((JsonObject)jo1["nested"])["level2"]);
            Assert.AreEqual(1, ((JsonObject)jo1["nested"]).Count);
        }
    }
}
