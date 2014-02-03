using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Alsing.Design;

public class ComponaCollectionEditor : CollectionEditor
{
	protected class ComponaCollectionForm : CollectionForm
	{
		private readonly ArrayList CreatedItems = new ArrayList();

		private readonly ComponaCollectionEditor Editor;

		private readonly CollectionEditorGui GUI;

		private readonly ArrayList RemovedItems = new ArrayList();

		private bool IsDirty;

		public ComponaCollectionForm(CollectionEditor e)
			: base(e)
		{
			Editor = e as ComponaCollectionEditor;
			if (Editor != null)
			{
				GUI = Editor.CreateGUI();
			}
			GUI.Visible = true;
			GUI.Dock = DockStyle.Fill;
			base.Controls.Add(GUI);
			base.AcceptButton = GUI.btnOK;
			base.CancelButton = GUI.btnCancel;
			base.Size = new Size(630, 470);
			GUI.Editor = e as ComponaCollectionEditor;
			Type[] newItemTypes = base.NewItemTypes;
			if (newItemTypes.Length > 1)
			{
				GUI.btnDropdown.Visible = true;
				GUI.btnDropdown.ContextMenu = new ContextMenu();
				for (int i = 0; i < newItemTypes.Length; i++)
				{
					GUI.btnDropdown.ContextMenu.MenuItems.Add(new TypeMenuItem(newItemTypes[i], btnDropDownMenuItem_Click));
				}
			}
			GUI.btnRemove.Click += btnRemove_Click;
			GUI.btnAdd.Click += btnAdd_Click;
			GUI.btnCancel.Click += btnCancel_Click;
			GUI.btnOK.Click += btnOK_Click;
			GUI.btnUp.Click += btnUp_Click;
			GUI.btnDown.Click += btnDown_Click;
			GUI.btnDropdown.Click += btnDropDown_Click;
		}

		public void RemoveObject(object o)
		{
			int num = GUI.lstMembers.Items.IndexOf(o);
			RemovedItems.Add(o);
			Editor.DestroyInstance(o);
			CreatedItems.Remove(o);
			GUI.lstMembers.Items.RemoveAt(GUI.lstMembers.SelectedIndex);
			IsDirty = true;
			if (num < GUI.lstMembers.Items.Count)
			{
				GUI.lstMembers.SelectedIndex = num;
			}
			else if (GUI.lstMembers.Items.Count > 0)
			{
				GUI.lstMembers.SelectedIndex = GUI.lstMembers.Items.Count - 1;
			}
		}

		public void AddObject(object o)
		{
			IList list = GUI.EditValue as IList;
			list.Add(o);
			IsDirty = true;
			GUI.lstMembers.Items.Add(o);
			CreatedItems.Add(o);
			if (o is Component)
			{
				Component component = o as Component;
				Editor.DesignerHost.Container.Add(component);
			}
			object[] array = new object[GUI.lstMembers.Items.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = GUI.lstMembers.Items[i];
			}
		}

		protected void btnUp_Click(object o, EventArgs e)
		{
			int selectedIndex = GUI.lstMembers.SelectedIndex;
			if (selectedIndex >= 1)
			{
				IsDirty = true;
				int topIndex = GUI.lstMembers.TopIndex;
				object obj = GUI.lstMembers.Items[selectedIndex];
				GUI.lstMembers.Items[selectedIndex] = GUI.lstMembers.Items[selectedIndex - 1];
				GUI.lstMembers.Items[selectedIndex - 1] = obj;
				if (topIndex > 0)
				{
					GUI.lstMembers.TopIndex = topIndex - 1;
				}
				GUI.lstMembers.ClearSelected();
				GUI.lstMembers.SelectedIndex = selectedIndex - 1;
			}
		}

		protected void btnDropDown_Click(object o, EventArgs e)
		{
			GUI.btnDropdown.ContextMenu.Show(GUI.btnDropdown, new Point(0, GUI.btnDropdown.Height));
		}

		protected void btnDropDownMenuItem_Click(object o, EventArgs e)
		{
			if (o is TypeMenuItem typeMenuItem)
			{
				CreateAndAddInstance(typeMenuItem.Type as Type);
			}
		}

		protected void btnDown_Click(object o, EventArgs e)
		{
			int selectedIndex = GUI.lstMembers.SelectedIndex;
			if (selectedIndex < GUI.lstMembers.Items.Count - 1 || selectedIndex < 0)
			{
				IsDirty = true;
				int topIndex = GUI.lstMembers.TopIndex;
				object obj = GUI.lstMembers.Items[selectedIndex];
				GUI.lstMembers.Items[selectedIndex] = GUI.lstMembers.Items[selectedIndex + 1];
				GUI.lstMembers.Items[selectedIndex + 1] = obj;
				if (topIndex < GUI.lstMembers.Items.Count - 1)
				{
					GUI.lstMembers.TopIndex = topIndex + 1;
				}
				GUI.lstMembers.ClearSelected();
				GUI.lstMembers.SelectedIndex = selectedIndex + 1;
			}
		}

		protected void btnRemove_Click(object o, EventArgs e)
		{
			int selectedIndex = GUI.lstMembers.SelectedIndex;
			RemovedItems.Add(GUI.lstMembers.SelectedItem);
			object selectedItem = GUI.lstMembers.SelectedItem;
			Editor.DestroyInstance(selectedItem);
			CreatedItems.Remove(selectedItem);
			GUI.lstMembers.Items.RemoveAt(GUI.lstMembers.SelectedIndex);
			IsDirty = true;
			if (selectedIndex < GUI.lstMembers.Items.Count)
			{
				GUI.lstMembers.SelectedIndex = selectedIndex;
			}
			else if (GUI.lstMembers.Items.Count > 0)
			{
				GUI.lstMembers.SelectedIndex = GUI.lstMembers.Items.Count - 1;
			}
		}

		protected void btnAdd_Click(object o, EventArgs e)
		{
			CreateAndAddInstance(base.NewItemTypes[0]);
		}

		protected void btnCancel_Click(object o, EventArgs e)
		{
			if (IsDirty)
			{
				foreach (object removedItem in RemovedItems)
				{
					DestroyInstance(removedItem);
				}
			}
			ClearAll();
		}

		protected void btnOK_Click(object o, EventArgs e)
		{
			if (IsDirty)
			{
				foreach (object removedItem in RemovedItems)
				{
					DestroyInstance(removedItem);
				}
				object[] array = new object[GUI.lstMembers.Items.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = GUI.lstMembers.Items[i];
				}
				base.Items = array;
			}
			ClearAll();
		}

		private void ClearAll()
		{
			CreatedItems.Clear();
			RemovedItems.Clear();
			IsDirty = false;
		}

		protected override void OnEditValueChanged()
		{
		}

		protected static void OnComponentChanged(object o, ComponentChangedEventArgs e)
		{
		}

		protected override DialogResult ShowEditorDialog(IWindowsFormsEditorService edSvc)
		{
			IComponentChangeService componentChangeService = null;
			DialogResult dialogResult = DialogResult.Cancel;
			GUI.EditorService = edSvc;
			try
			{
				componentChangeService = (IComponentChangeService)Editor.Context.GetService(typeof(IComponentChangeService));
				if (componentChangeService != null)
				{
					componentChangeService.ComponentChanged += OnComponentChanged;
				}
				GUI.EditValue = base.EditValue;
				GUI.Bind();
				GUI.ActiveControl = GUI.lstMembers;
				base.ActiveControl = GUI;
				return base.ShowEditorDialog(edSvc);
			}
			finally
			{
				if (componentChangeService != null)
				{
					componentChangeService.ComponentChanged -= OnComponentChanged;
				}
			}
		}

		private void CreateAndAddInstance(Type type)
		{
			try
			{
				object obj = CreateInstance(type);
				if (obj != null)
				{
					IsDirty = true;
					CreatedItems.Add(obj);
					GUI.lstMembers.Items.Add(obj);
					GUI.lstMembers.Invalidate();
					GUI.lstMembers.ClearSelected();
					GUI.lstMembers.SelectedIndex = GUI.lstMembers.Items.Count - 1;
					object[] array = new object[GUI.lstMembers.Items.Count];
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = GUI.lstMembers.Items[i];
					}
					base.Items = array;
				}
				IsDirty = true;
			}
			catch (Exception e)
			{
				base.DisplayError(e);
			}
		}
	}

	public class TypeMenuItem : MenuItem
	{
		public object Type { get; set; }

		public TypeMenuItem(object o, EventHandler e)
		{
			base.Text = o.ToString();
			Type = o;
			base.Click += e;
		}
	}

	private ComponaCollectionForm Form;

	public IDesignerHost DesignerHost => (IDesignerHost)GetService(typeof(IDesignerHost));

	public ComponaCollectionEditor(Type t)
		: base(t)
	{
	}

	public void AddObject(object o)
	{
		Form.AddObject(o);
	}

	public void RemoveObject(object o)
	{
		Form.RemoveObject(o);
	}

	protected virtual CollectionEditorGui CreateGUI()
	{
		return new CollectionEditorGui();
	}

	protected override CollectionForm CreateCollectionForm()
	{
		Form = new ComponaCollectionForm(this)
		{
			StartPosition = FormStartPosition.CenterScreen
		};
		return Form;
	}
}
