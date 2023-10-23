using System.Collections.Generic;
using System.Linq;
using DataAssembly;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using Prepatcher;
using Verse;

namespace MutationTracing;

public static class FieldPrepatches
{
    [FreePatchAll]
    static bool PatchFieldStores(ModuleDefinition module)
    {
        if (module.Name != "Assembly-CSharp.dll")
            return false;

        var fieldDescs = new List<string>();
        foreach (var m in module.Types.SelectMany(t => t.Methods))
            if (m.HasBody && !m.IsConstructor)
            {
                for (int j = m.Body.Instructions.Count() - 1; j >= 0; j--)
                {
                    var inst = m.Body.Instructions.ElementAt(j);
                    if (inst.OpCode == OpCodes.Stfld ||
                        inst.OpCode == OpCodes.Stsfld)
                    {
                        if (inst.ToString().Contains("<>")) continue; // Ignore fields storing compiler-generated objects

                        m.Body.Instructions.InsertRange(
                            j + 1,
                            new[]
                            {
                                Instruction.Create(OpCodes.Ldc_I4, fieldDescs.Count),
                                Instruction.Create(OpCodes.Call,
                                    module.ImportReference(typeof(DataClass).GetMethod(nameof(DataClass.Record)))),
                            }
                        );

                        fieldDescs.Add(((FieldReference)inst.Operand).DeclaringType.FullName + "." + ((FieldReference)inst.Operand).Name);
                    }
                }

                m.FixShortLongOps();
            }

        Log.Message($"Mutation tracing: Field sites for assembly {module.Name}: {fieldDescs.Count}");

        DataClass.fieldsInterface = new string[fieldDescs.Count];
        DataClass.fieldsSimulation = new string[fieldDescs.Count];
        DataClass.fieldDescs = fieldDescs.ToArray();

        return true;
    }

    static bool IsMutatingMethod(Instruction inst)
    {
        if (inst.Operand is MethodReference { Name: "Add" or "Enqueue", HasThis: true } m)
        {
            int stck = 0;
            while (inst.Previous != null)
            {
                inst = inst.Previous;
                CodeWriter.ComputeStackDelta(inst, ref stck);
                if (stck == 1 + m.Parameters.Count())
                    return inst.OpCode == OpCodes.Ldfld || inst.opcode == OpCodes.Ldsfld;
            }
        }

        return false;
    }
}
