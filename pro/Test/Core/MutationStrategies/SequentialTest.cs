﻿using System.IO;
using NUnit.Framework;
using Peach.Core;
using Peach.Core.Analyzers;
using Peach.Core.Test;
using Peach.Pro.Core.MutationStrategies;

namespace Peach.Pro.Test.Core.MutationStrategies
{
	[TestFixture] 
	[Peach]
	[Quick]
	class SequentialTest : DataModelCollector
	{
		static readonly string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<String name='str1' value='Hello World!' />
		<String name='str2' value='Hello World!' />
		<String name='str3' value='Hello World!' />
		<String name='str4' value='Hello World!' />
		<String name='str5' value='Hello World!' />
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel' />
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState' />
		<Publisher class='Null' />
		<Strategy class='Sequential' />
		<Mutators mode='include'>
			<Mutator class='DataElementRemove' />
			<Mutator class='StringCaseLower' />
			<Mutator class='StringCaseUpper' />
		</Mutators>
	</Test>
</Peach>";

		[Test]
		public void Test1()
		{
			RunEngine(xml);

			// 5 strings, 3 mutators, 1 mutation each
			Assert.AreEqual(5 * 3, mutations.Count);
			Assert.AreEqual(5 * 3, strategies.Count);

			// this strategy fuzzes elements in a sequential order
			string[] expected = {
									"DataElementRemove | TheDataModel.str1",
									"StringCaseLower | TheDataModel.str1",
									"StringCaseUpper | TheDataModel.str1",
									"DataElementRemove | TheDataModel.str2",
									"StringCaseLower | TheDataModel.str2",
									"StringCaseUpper | TheDataModel.str2",
									"DataElementRemove | TheDataModel.str3",
									"StringCaseLower | TheDataModel.str3",
									"StringCaseUpper | TheDataModel.str3",
									"DataElementRemove | TheDataModel.str4",
									"StringCaseLower | TheDataModel.str4",
									"StringCaseUpper | TheDataModel.str4",
									"DataElementRemove | TheDataModel.str5",
									"StringCaseLower | TheDataModel.str5",
									"StringCaseUpper | TheDataModel.str5",
								};

			Assert.AreEqual(expected, strategies);
		}

		public void FuzzSequential(RunConfiguration config)
		{
			var dom = ParsePit(xml);

			var e = new Engine(this);
			e.startFuzzing(dom, config);
		}

		[Test]
		public void Test2()
		{
			// Tests skipping to the 0th iteration
			var config = new RunConfiguration();
			config.skipToIteration = 0;
			FuzzSequential(config);
			Assert.AreEqual(16, values.Count);
			Assert.AreEqual(15, mutations.Count);
		}

		[Test]
		public void Test3()
		{
			// Tests skipping to the 1st iteration
			var config = new RunConfiguration();
			config.skipToIteration = 1;
			FuzzSequential(config);
			Assert.AreEqual(16, values.Count);
			Assert.AreEqual(15, mutations.Count);
		}

		[Test]
		public void Test3a()
		{
			// Tests skipping to the 2nd iteration
			var config = new RunConfiguration();
			config.skipToIteration = 2;
			FuzzSequential(config);
			Assert.AreEqual(15, values.Count);
			Assert.AreEqual(14, mutations.Count);
		}

		[Test]
		public void Test4()
		{
			// Tests skipping a middle iteration
			var config = new RunConfiguration();
			config.skipToIteration = 12;
			FuzzSequential(config);
			Assert.AreEqual(5, values.Count);
			Assert.AreEqual(4, mutations.Count);
		}

		[Test]
		public void Test4a()
		{
			// Tests skipping to the last iteration
			var config = new RunConfiguration();
			config.skipToIteration = 15;
			FuzzSequential(config);
			Assert.AreEqual(2, values.Count);
			Assert.AreEqual(1, mutations.Count);
		}

		[Test]
		public void Test5()
		{
			// Tests skipping past the last iteration
			var config = new RunConfiguration();
			config.skipToIteration = 16;
			FuzzSequential(config);
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(0, mutations.Count);
		}

		[Test]
		public void Test6()
		{
			// Tests skipping way past the last iteration
			var config = new RunConfiguration();
			config.skipToIteration = 30;
			FuzzSequential(config);
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(0, mutations.Count);
		}

		[Test]
		public void Test7()
		{
			var config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 5;
			config.rangeStop = 5;
			FuzzSequential(config);
			Assert.AreEqual(2, values.Count);
			Assert.AreEqual(1, mutations.Count);
		}

		[Test]
		public void Test8()
		{
			var config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 5;
			config.rangeStop = 10;
			FuzzSequential(config);
			Assert.AreEqual(7, values.Count);
			Assert.AreEqual(6, mutations.Count);
		}

		[Test]
		public void TestSequential()
		{
			var dom = ParsePit(xml);
			var strategy = dom.tests[0].strategy;
			Assert.IsNotNull(strategy);
			Assert.IsInstanceOf<Sequential>(strategy);
		}

		public uint GetMutationCount(string data)
		{
			string template = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<Defaults>
		<Number endian='big'/>
	</Defaults>

	<DataModel name='choice_string'>
		<Number name='string_type' size='8' token='true' value='1'/>
		<Number name='string_size' size='32'>
			<Relation type='size' of='string_data' />
		</Number>
		<String name='string_data'/>
	</DataModel>

	<DataModel name='choice_blob'>
		<Number name='blob_type' size='8' token='true' value='2'/>
		<Number name='blob_size' size='32'>
			<Relation type='size' of='blob_data' />
		</Number>
		<Blob name='blob_data'/>
	</DataModel>

	<DataModel name='choice_number'>
		<Number name='num_type' size='8' token='true' value='3'/>
		<Number name='num_data' size='32'/>
	</DataModel>

	<DataModel name='TheDataModel'>
		<Choice name='choice'>
			<Block name='choice_string' ref='choice_string'/>
			<Block name='choice_blob' ref='choice_blob'/>
			<Block name='choice_number' ref='choice_number'/>
		</Choice>
		<String name='str' value='Hello World!'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
				<Data fileName='{0}'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
		<Strategy class='Sequential'/>
	</Test>
</Peach>";

			string tempFile = Path.GetTempFileName();
			File.WriteAllBytes(tempFile, Encoding.ASCII.GetBytes(data));

			string xml = string.Format(template, tempFile);

			PitParser parser = new PitParser();
			Peach.Core.Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;
			Engine e = new Engine(this);
			e.startFuzzing(dom, config);

			return dom.tests[0].strategy.Count;
		}

		[Test]
		public void TestChoice()
		{
			uint str = GetMutationCount("\x01\x00\x00\x00\x05Hello");
			uint blob = GetMutationCount("\x02\x00\x00\x00\x05Hello");
			uint num = GetMutationCount("\x03\x00\x01\x02\x03");

			Assert.AreNotEqual(str, blob);
			Assert.AreNotEqual(str, num);
			Assert.AreNotEqual(blob, num);
		}

		[Test]
		public void FieldOverride()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String name='str1' value='000' length='3' mutable='false'/>
		<String name='str2' value='Hello'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
				<Data>
					<Field name='str1' value='111'/>
				</Data>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
		<Strategy class='Sequential'/>
		<Mutators mode='include'>
			<Mutator class='StringCaseLower'/>
		</Mutators>
	</Test>
</Peach>";

			RunEngine(xml);

			// 1 control, 1 mutation
			Assert.AreEqual(2, dataModels.Count);

			for (int i = 0; i < dataModels.Count; ++i)
			{
				string val = (string)dataModels[i][0].InternalValue;
				Assert.AreEqual("111", val);
			}
		}

		[Test]
		public void ReEnterState()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='num' size='8' mutable='false'>
			<Fixup class='SequenceIncrementFixup'>
				<Param name='Offset' value='0'/>
			</Fixup>
		</Number>
		<String name='str'/>
	</DataModel>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
				<Data>
					<Field name='str' value='Hello'/>
				</Data>
			</Action>
			<Action type='changeState' ref='Second'/>
		</State>

		<State name='Second'>
			<Action type='output'>
				<DataModel ref='DM'/>
				<Data>
					<Field name='str' value='World'/>
				</Data>
			</Action>
			<Action type='changeState' ref='Initial' when='int(state.actions[0].dataModel[&quot;num&quot;].InternalValue) &lt; 4'/>
		</State>

	</StateModel>

	<Test name='Default'>
		<StateModel ref='SM'/>
		<Publisher class='Null'/>
		<Strategy class='Sequential'/>
		<Mutators mode='include'>
			<Mutator class='StringCaseLower' />
			<Mutator class='StringCaseUpper' />
		</Mutators>
	</Test>
</Peach>";

			RunEngine(xml);
			
			// 4 DM for control
			// 2 mutations per field, 4 fields = 8 iterations
			// 8 iterations * 4 DM per = 32
			// 36 total

			Assert.AreEqual(36, dataModels.Count);

			Assert.AreEqual("Hello", (string)dataModels[0][1].InternalValue);
			Assert.AreEqual("World", (string)dataModels[1][1].InternalValue);
			Assert.AreEqual("Hello", (string)dataModels[2][1].InternalValue);
			Assert.AreEqual("World", (string)dataModels[3][1].InternalValue);

			int total = 0;
			for (int i = 4; i < 36; i += 4)
			{
				// For any given iteration, only 1 field should be mutated
				int changed = 0;

				if ("Hello" != (string)dataModels[i + 0][1].InternalValue)
					++changed;
				if ("World" != (string)dataModels[i + 1][1].InternalValue)
					++changed;
				if ("Hello" != (string)dataModels[i + 2][1].InternalValue)
					++changed;
				if ("World" != (string)dataModels[i + 3][1].InternalValue)
					++changed;

				// one element should change each iteration
				Assert.AreEqual(1, changed);
				total += changed;
			}

			// 12 total iterations of fuzzing
			Assert.AreEqual(8, total);
		}

	}
}
