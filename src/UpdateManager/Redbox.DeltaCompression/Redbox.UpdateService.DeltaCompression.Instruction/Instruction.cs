namespace Redbox.UpdateService.DeltaCompression.Instruction;

public class Instruction
{
	public InstructionType Type { get; set; }

	public string Key { get; set; }

	public string Value { get; set; }

	public byte[] CacheValue { get; set; }

	public int StartIndex { get; set; }

	public static Instruction CreateUpdate(string key, string value, int startIndex)
	{
		Instruction instruction = new Instruction();
		instruction.Type = InstructionType.Update;
		instruction.Key = key;
		instruction.Value = value;
		instruction.StartIndex = startIndex;
		return instruction;
	}

	public static Instruction CreateInsert(string key, string value)
	{
		Instruction instruction = new Instruction();
		instruction.Type = InstructionType.Insert;
		instruction.Key = key;
		instruction.Value = value;
		return instruction;
	}

	public static Instruction CreateDelete(string key)
	{
		Instruction instruction = new Instruction();
		instruction.Type = InstructionType.Delete;
		instruction.Key = key;
		return instruction;
	}

	public static Instruction CreateCacheUpdateInsert(string key, byte[] data)
	{
		Instruction instruction = new Instruction();
		instruction.Type = InstructionType.CacheUpdateInsert;
		instruction.Key = key;
		instruction.CacheValue = data;
		return instruction;
	}

	public static Instruction CreateCacheDelete(string key)
	{
		Instruction instruction = new Instruction();
		instruction.Type = InstructionType.CacheDelete;
		instruction.Key = key;
		return instruction;
	}
}
