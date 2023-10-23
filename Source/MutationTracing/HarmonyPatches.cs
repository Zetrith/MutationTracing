using System;
using System.IO;
using System.Security;
using System.Text;
using DataAssembly;
using HarmonyLib;
using Multiplayer.Client;
using Verse;

using static Multiplayer.Client.Multiplayer;

namespace MutationTracing;

[HarmonyPatch(typeof(HostUtil), "StartLocalServer")]
internal static class OnHostServer
{
    private static string StateString => $"cl:{Client != null} tck:{Ticking} ex:{ExecutingCmds} re:{reloading} ps:{Current.ProgramState} le:{LongEventHandler.currentEvent?.eventText ?? "None"} le_wait:{LongEventHandler.ShouldWaitForEvent}";

    private static void Postfix()
    {
        DataClass.StateString = static () => StateString;
        DataClass.ShouldRecord = static () => !reloading && LongEventHandler.currentEvent == null && Current.ProgramState == ProgramState.Playing;
        DataClass.InInterface = static () => InInterface;
    }
}

[HarmonyPatch(typeof(Multiplayer.Client.Multiplayer), nameof(StopMultiplayer))]
internal static class OnMultiplayerStop
{
    private static void Postfix()
    {
        if (DataClass.InInterface != null)
        {
            WriteMutationTraces();

            DataClass.InInterface = null;
            DataClass.StateString = null;
            DataClass.ShouldRecord = null;
        }
    }

    private static void WriteMutationTraces()
    {
        StringBuilder xml = new StringBuilder();
        xml.Append(@"<?xml version=""1.0""?>
				<doc>
			<assembly>
			<name>Assembly-CSharp</name>
			</assembly><members>");

        for (int i = 0; i < DataClass.fieldDescs.Length; i++)
        {
            if (DataClass.fieldsInterface[i] != null || DataClass.fieldsSimulation[i] != null)
            {
                xml.Append(@$"<member name=""F:{SecurityElement.Escape(DataClass.fieldDescs[i])}"">");
                if (DataClass.fieldsInterface[i] != null)
                    xml.Append("Interface");
                if (DataClass.fieldsSimulation[i] != null)
                    xml.Append("Simulation");
                if (DataClass.fieldsInterface[i] != null)
                {
                    xml.Append("\n<code>").Append(SecurityElement.Escape(DataClass.fieldsInterface[i])).Append("\n</code>");
                }
                if (DataClass.fieldsSimulation[i] != null)
                {
                    xml.Append("\n<code>").Append(SecurityElement.Escape(DataClass.fieldsSimulation[i])).Append("\n</code>");
                }
                xml.Append("</member>\n");
            }
        }

        xml.Append("</members></doc>");

        File.WriteAllText($"Fields_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.xml", xml.ToString());
    }
}
