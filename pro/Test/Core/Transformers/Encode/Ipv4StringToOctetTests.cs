﻿using System.IO;
using NUnit.Framework;
using Peach.Core;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.Test;

namespace Peach.Pro.Test.Core.Transformers.Encode
{
	[TestFixture]
	[Quick]
	[Peach]
    class Ipv4StringToOctetTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test

			string xml = @"
				<Peach>
					<DataModel name='TheDataModel'>
						<Block name='TheBlock'>
							<Transformer class='Ipv4StringToOctet'/>
							<Blob name='Data' value='192.168.1.1'/>
						 </Block>
					</DataModel>

					<StateModel name='TheState' initialState='Initial'>
						<State name='Initial'>
							<Action type='output'>
								<DataModel ref='TheDataModel'/>
							</Action>
						</State>
					</StateModel>

					<Test name='Default'>
						<StateModel ref='TheState'/>
						<Publisher class='Null'/>
					</Test>
				</Peach>";

            PitParser parser = new PitParser();

            Peach.Core.Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();
            config.singleIteration = true;

            Engine e = new Engine(this);
            e.startFuzzing(dom, config);

            // verify values
            // -- this is the pre-calculated result from Peach2.3 on the blob: "192.168.1.1"
            byte[] precalcResult = new byte[] { 0xC0, 0xA8, 0x01, 0x01 };
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcResult, values[0].ToArray());
        }

		[Test]
		public void InvalidIPAdressTest()
		{

			string xml = @"
				<Peach>
					<DataModel name='TheDataModel'>
						<Block name='TheBlock'>
							<Transformer class='Ipv4StringToOctet'/>
							<Blob name='Data' value='192.168.1'/>
						 </Block>
					</DataModel>

					<StateModel name='TheState' initialState='Initial'>
						<State name='Initial'>
							<Action type='output'>
								<DataModel ref='TheDataModel'/>
							</Action>
						</State>
					</StateModel>

					<Test name='Default'>
						<StateModel ref='TheState'/>
						<Publisher class='Null'/>
					</Test>
				</Peach>";

			PitParser parser = new PitParser();

			Peach.Core.Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(this);
			var ex = Assert.Throws<PeachException>(() => e.startFuzzing(dom, config));
			Assert.AreEqual("Error, can't transform IP to bytes, '192.168.1' is not a valid IP address.", ex.Message);
		}

		[Test]
		public void CrackingTest1()
		{
			string xml = @"
						<Peach>
							<DataModel name='TheDataModel'>
								<String name='Data' value='192.168.1.1'>
									<Transformer class='Ipv4StringToOctet'/>
								</String>
							</DataModel>
						 </Peach>";

			PitParser parser = new PitParser();
			Peach.Core.Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", new byte[] { 0xC0, 0xA8, 0x01, 0x01 });

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("192.168.1.1", (string)dom.dataModels[0][0].DefaultValue);
		}

		[Test]
		public void CrackingTest2()
		{
			string xml = @"
						<Peach>
							<DataModel name='DM'>
								<Block name='blk' length='4'>
									<String name='IP'>
										<Transformer class='Ipv4StringToOctet'/>
									</String>
								</Block>
								<String name='Payload'/>
							</DataModel>
						</Peach>";

			PitParser parser = new PitParser();
			Peach.Core.Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}{1}", new byte[] { 0xC0, 0xA8, 0x01, 0x01 }, "Hello");

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("192.168.1.1", (string)dom.dataModels[0].find("blk.IP").DefaultValue);
			Assert.AreEqual("Hello", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void NotEnoughDataCrackingTest()
		{
			string xml = @"
						<Peach>
							<DataModel name='DM'>
									<String name='IP'>
										<Transformer class='Ipv4StringToOctet'/>
									</String>
							</DataModel>
						</Peach>";

			PitParser parser = new PitParser();
			Peach.Core.Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", new byte[] { 0x3F, 0xFE, 0x19 });

			DataCracker cracker = new DataCracker();
			var ex = Assert.Throws<PeachException>(() => cracker.CrackData(dom.dataModels[0], data));
			Assert.AreEqual("Error, can't transform bytes to IP, expected 4 bytes but got 3 bytes.", ex.Message);
		}

		[Test]
		public void TooMuchDataCrackingTest()
		{
			string xml = @"
						<Peach>
							<DataModel name='DM'>
									<String name='IP'>
										<Transformer class='Ipv4StringToOctet'/>
									</String>
							</DataModel>
						</Peach>";

			PitParser parser = new PitParser();
			Peach.Core.Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", new byte[] { 0x3F, 0xFE, 0x19, 0x3F, 0xFE, 0x19, 0xFF });

			DataCracker cracker = new DataCracker();
			var ex = Assert.Throws<PeachException>(() => cracker.CrackData(dom.dataModels[0], data));
			Assert.AreEqual("Error, can't transform bytes to IP, expected 4 bytes but got 7 bytes.", ex.Message);
		}
	}
}

// end
