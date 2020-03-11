using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;

namespace ROMSpinner.LairUI
{
    public class TickTypeEditor : System.Drawing.Design.UITypeEditor
    {
        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return System.Drawing.Design.UITypeEditorEditStyle.Modal;
            //return base.GetEditStyle(context);
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            GeneralSequenceProperties prop = (GeneralSequenceProperties) context.Instance;
            uint uTicks = prop.GetTicks();

            TickDialog dlg = new TickDialog();
            dlg.ShowDialog();

            return base.EditValue(context, provider, value);
        }
    }

    public class PointsTypeEditor : System.Drawing.Design.UITypeEditor
    {
        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return System.Drawing.Design.UITypeEditorEditStyle.Modal;
            //return base.GetEditStyle(context);
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            /*
            GeneralSequenceProperties prop = (GeneralSequenceProperties)context.Instance;
            uint uTicks = prop.GetTicks();

            TickDialog dlg = new TickDialog();
            dlg.ShowDialog();
             */

            return base.EditValue(context, provider, value);
        }
    }

}
