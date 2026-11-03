using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Mirror;

namespace ZetymPatch;

class StatisticsManagerPatches
{
	[HarmonyPatch(typeof(StatisticsManager), "ClearTodaysValues")]
	class Patch01
	{
		static void Prefix(out int __state)
		{
			__state = StatisticsManager.Instance.productsSold.Count;
		}
		static void Postfix(int __state)
		{
			var sm = StatisticsManager.Instance;
			for(int i = sm.productsSold.Count; i < __state; i++)
			{
				sm.productsSold.Add(0);
				sm.revenuePerProductSold.Add(0);
				sm.productsAcquired.Add(0);
				sm.costPerProductAcquired.Add(0);
			}
		}
	}

	[HarmonyPatch(typeof(StatisticsManager), nameof(StatisticsManager.SaveStatistics))]
	class Patch02
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codeMatcher = new CodeMatcher(instructions).Start();

			codeMatcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "day"))
				.MatchForward(false, new CodeMatch(OpCodes.Call))
				.ThrowIfInvalid("Failed matching");
			codeMatcher.Advance(-1).SetOpcodeAndAdvance(OpCodes.Ldfld);
			codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Sub));
			codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, 7))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 7));

			return codeMatcher.Instructions();
		}
	}
}

class ManagerBlackboardPatches
{
	[HarmonyPatch]
	class Patch01
	{
		static IEnumerable<MethodBase> TargetMethods()
		{
			var serverCargoSpawner = AccessTools.Method(typeof(ManagerBlackboard), "ServerCargoSpawner");
			var cargoMoveNext = AccessTools.EnumeratorMoveNext(serverCargoSpawner);
			
			yield return cargoMoveNext;

			var serverAutosaveBoxesSpawn = AccessTools.Method(typeof(ManagerBlackboard), "ServerAutosaveBoxesSpawn");
			var autosaveMoveNext = AccessTools.EnumeratorMoveNext(serverAutosaveBoxesSpawn);

			yield return autosaveMoveNext;			
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codeMatcher = new CodeMatcher(instructions);
			var smType = typeof(StatisticsManager);
			var acquiredField = smType.GetField(nameof(StatisticsManager.productsAcquired));
			var costField = smType.GetField(nameof(StatisticsManager.costPerProductAcquired));
			var mbType = typeof(ManagerBlackboard);
			var boxPriceMethod = mbType.GetMethod("PricePerBoxRetrieve", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);


			CodeMatch[] matches = [
				new CodeMatch(OpCodes.Ldfld, acquiredField),
				new CodeMatch(OpCodes.Add),
				new CodeMatch(OpCodes.Ldfld, costField),
				new CodeMatch(OpCodes.Call, boxPriceMethod)
			];

			
			codeMatcher.Start();
			codeMatcher.MatchForward(false, matches[0]);
			codeMatcher.MatchForward(false, matches[1]);
			codeMatcher.ThrowIfInvalid("Failed first matching");
			codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 100));
			codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Mul));

			codeMatcher.MatchForward(false, matches[2]);
			codeMatcher.MatchForward(false, matches[3]);
			codeMatcher.ThrowIfInvalid("Failed second matching");
			codeMatcher.Advance(1);
			codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_R4, 100.0f));
			codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Mul));
			var r = UnityEngine.Mathf.Round;
			codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, r.GetMethodInfo()));

			codeMatcher.ThrowIfInvalid("something failed");
			return codeMatcher.Instructions();
		}
	}
}

class ProductCheckoutSpawnPatches
{
	[HarmonyPatch(typeof(ProductCheckoutSpawn), "AddProductShared")]
	class Patch01
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
		{
			var codeMatcher = new CodeMatcher(instructions, ilGenerator);
			var smType = typeof(StatisticsManager);
			var soldField = smType.GetField(nameof(StatisticsManager.productsSold));
			var revField = smType.GetField(nameof(StatisticsManager.revenuePerProductSold));
			var pcsType = typeof(ProductCheckoutSpawn);
			var carryField = pcsType.GetField(nameof(ProductCheckoutSpawn.productCarryingPrice));

			CodeMatch[] matches = [
				new CodeMatch(OpCodes.Ldfld, soldField),
				new CodeMatch(OpCodes.Ldc_I4_1),
				new CodeMatch(OpCodes.Ldfld, revField),
				new CodeMatch(OpCodes.Ldfld, carryField)
			];

			codeMatcher.Start();
			codeMatcher.MatchForward(false, matches[0]);
			codeMatcher.MatchForward(false, matches[1]);
			codeMatcher.ThrowIfInvalid("Failed first matching");
			codeMatcher.Set(OpCodes.Ldc_I4, 100);

			codeMatcher.MatchForward(false, matches[2]);
			codeMatcher.MatchForward(false, matches[3]);
			codeMatcher.Advance(1);
			codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_R4, 100.0f));
			codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Mul));
			var r = UnityEngine.Mathf.Round;
			codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, r.GetMethodInfo()));

			codeMatcher.ThrowIfInvalid("Failed second matching");
			return codeMatcher.Instructions();
		}
	}
}

class StatisticsDeskPatches
{
	[HarmonyPatch(typeof(StatisticsDesk), nameof(StatisticsDesk.RedrawGraphs))]
	class Patch03
	{
		static void Prefix(ref string statisticsData, bool isAProductList)
		{
			if(isAProductList)
			{
				string[] array = statisticsData.Split("|");
				StringBuilder sb = new StringBuilder();
				for (int j = 0; j < array.Length; j++)
				{
					if (!(array[j] == "") && array[j].Contains("*"))
					{
						string[] array2 = array[j].Split("*");
						float num2 = float.Parse(array2[1]);
						num2 /= 100.0f;
						sb.Append(array2[0] + "*" + num2.ToString("F2") + "|");
					}
				}
				statisticsData = sb.ToString();
			}
		}
	}
}
