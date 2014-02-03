using System.Collections;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
using Alsing.SourceCode;

namespace Alsing.Windows.Forms.SyntaxBox;

public class SyntaxBoxDesigner : ControlDesigner
{
	protected ISelectionService SelectionService => (ISelectionService)GetService(typeof(ISelectionService));

	protected virtual IDesignerHost DesignerHost => (IDesignerHost)base.GetService(typeof(IDesignerHost));

	public override void InitializeNewComponent(IDictionary defaultValues)
	{
		base.InitializeNewComponent(defaultValues);
		if (DesignerHost != null)
		{
			DesignerTransaction designerTransaction = DesignerHost.CreateTransaction("Adding Syntaxdocument");
			SyntaxDocument document = DesignerHost.CreateComponent(typeof(SyntaxDocument)) as SyntaxDocument;
			if (!(Control is SyntaxBoxControl syntaxBoxControl))
			{
				designerTransaction.Cancel();
				return;
			}
			syntaxBoxControl.Document = document;
			designerTransaction.Commit();
		}
	}
}
