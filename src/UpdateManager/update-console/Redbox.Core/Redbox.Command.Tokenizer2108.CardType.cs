namespace Redbox.Core;

internal enum Redbox_002ECommand_002ETokenizer2108_002ECardType : byte
{
	Unknown,
	[Redbox_002ECommand_002ETokenizer2108_002ECardCode(Code = "AX")]
	AmericanExpress,
	[Redbox_002ECommand_002ETokenizer2108_002ECardCode(Code = "DI")]
	Discover,
	[Redbox_002ECommand_002ETokenizer2108_002ECardCode(Code = "JC")]
	JCB,
	[Redbox_002ECommand_002ETokenizer2108_002ECardCode(Code = "MC")]
	MasterCard,
	[Redbox_002ECommand_002ETokenizer2108_002ECardCode(Code = "VI")]
	Visa
}
