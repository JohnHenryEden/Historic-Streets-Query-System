using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OldCityBuild0
{
    public partial class PopupWindow : Form
    {
        public PopupWindow()
        {
            InitializeComponent();
        }
        public HTMLPopupDisplayClass pHtmlPopupDisplay = new HTMLPopupDisplayClass();
        private void PopupWindow_Load(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add(pHtmlPopupDisplay.FieldNames.Count);
            
            
            string[] fieldName = pHtmlPopupDisplay.FieldNames.ToArray();
            string[] fieldValue = pHtmlPopupDisplay.FieldValues.ToArray();
            string[] attachmentImagesLink = pHtmlPopupDisplay.AttachLinks.ToArray();

            for (int i = 0; i < pHtmlPopupDisplay.FieldNames.Count; i++)  
            {
                dataGridView1.Rows[i].Cells[0].Value = fieldName[i];
                dataGridView1.Rows[i].Cells[1].Value = fieldValue[i];
            }

            if (pHtmlPopupDisplay.AttachLinks.Count > 0)
            {
                dataGridView2.Rows.Add(pHtmlPopupDisplay.AttachLinks.Count);
                for (int j = 0; j < pHtmlPopupDisplay.AttachLinks.Count; j++)
                {
                    Image.GetThumbnailImageAbort abortCallback = new Image.GetThumbnailImageAbort(this.target);
                    dataGridView2.Rows[j].Cells[0].Value = Image.FromFile(attachmentImagesLink[j]).GetThumbnailImage(512,400, abortCallback, System.IntPtr.Zero);
                }
            }
        }

        private bool target() { return false; }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void hTMLPopupDisplayClassBindingSource1_CurrentChanged(object sender, EventArgs e)
        {

        }
    }
}
