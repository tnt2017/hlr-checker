using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Globalization;

namespace hlr_checker
{
    public partial class Form1 : Form
    {
        string colname = "A4";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //@"D:\пробники_22052017.csv";


            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "D:\\пробники_22052017\\";
            openFileDialog1.Filter = "csv files (*.csv)|*.csv|txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            dataGridView1.DataSource = ReadCSVFile(openFileDialog1.FileName); // @"C:\Users\User\Desktop\jili-bili.csv"
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }

            try
            {
                dataGridView1.Columns["E0"].Width = 150;
                dataGridView1.Columns["E1"].Width = 150;
                dataGridView1.Columns["E2"].Width = 400;
            }
            catch
            {

            }

            colname = textBox_column.Text;
        }

        private DataTable ReadCSVFile(string pathToCsvFile)
        {
            //создаём таблицу
            DataTable dt = new DataTable("Cars");

            try
            {
                DataRow dr = null;
                string[] carValues = null;
                string[] cars = File.ReadAllLines(pathToCsvFile, Encoding.GetEncoding(1251));
                carValues = cars[0].Split(';'); // названия столбцов
                
                //MessageBox.Show(carValues.Length.ToString());

                for (int i = 0; i < carValues.Length; i++)
                {
                    dt.Columns.Add("A"+i, typeof(String));
                }

                for (int i = 0; i < 3; i++)
                {
                    dt.Columns.Add("E" + i, typeof(String));
                }

                for (int i = 0; i < cars.Length; i++)
                {
                    if (!String.IsNullOrEmpty(cars[i]))
                    {
                        carValues = cars[i].Split(';');
                        //carValues.Length;
                        //создаём новую строку
                        dr = dt.NewRow();

                        for (int j = 0; j < carValues.Length; j++)
                        {
                            dr["A" + j] = carValues[j];
                        }

                        //добавляем строку в таблицу
                        dt.Rows.Add(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dt;
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.RowCount - 1; i++)
            {
                string s = dataGridView1.Rows[i].Cells[colname].Value.ToString();
                s = s.Replace("-", "");
                s = s.Replace("\"", "");
                s = s.Replace(" ", "");

                if (s.Length == 10)
                    s = "7" + s;

                dataGridView1.Rows[i].Cells[colname].Value = s;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
                colname = textBox_column.Text;
                string phone = dataGridView1.CurrentRow.Cells[colname].Value.ToString();
                //MessageBox.Show(phone);

                string site = "https://smsc.ru/sys/send.php?login=idbonuskz&psw=FZ9Zabk8Z&phones=" + phone + "&hlr=1";

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(site);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                using (StreamReader stream = new StreamReader(
                     resp.GetResponseStream(), Encoding.UTF8))
                {
                    string Text = stream.ReadToEnd();
                    dataGridView1.CurrentRow.Cells["E0"].Value = Text;
                    //MessageBox.Show(Text);
                }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            colname = textBox_column.Text;
            string phone = dataGridView1.CurrentRow.Cells[colname].Value.ToString();
            string ID = dataGridView1.CurrentRow.Cells[textBox_id_column.Text].Value.ToString();

            if (ID == "")
            {
                    MessageBox.Show("Не отправили HLR запрос");
                    return;
            }

            try
            {
                ID = ID.Substring(ID.Length - 6, 6);
            }
            catch
            {

            }
                SMSC smsc = new SMSC();

                string[] r = smsc.get_status(ID, phone, 2);

                //MessageBox.Show(r.Count().ToString());
                string Text = "";
                foreach (string s in r)
                {
                    Text += s + "\r\n";
                }

                //MessageBox.Show(Text);

                if (r.Count() > 15)
                {
                    dataGridView1.CurrentRow.Cells["E0"].Value = r[15];
                    dataGridView1.CurrentRow.Cells["E1"].Value = r[18];
                    dataGridView1.CurrentRow.Cells["E2"].Value = r[19];
                //textBox_region_column
            }

                Application.DoEvents();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            progressBar1.Maximum = dataGridView1.RowCount;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                dataGridView1.CurrentCell = dataGridView1[0, i];
                button3_Click(null, null);
                this.Text = "HLR CHECKER " +  i.ToString() + " из " + dataGridView1.RowCount.ToString();
                progressBar1.Value = i;
                Application.DoEvents();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            progressBar1.Maximum = dataGridView1.RowCount;
            for (int i = Convert.ToInt32(numericUpDown1.Value); i < dataGridView1.RowCount; i++)
            {
                dataGridView1.CurrentCell = dataGridView1[0, i];
                this.Text = "HLR CHECKER " + i.ToString() + " из " + dataGridView1.RowCount.ToString();
                button4_Click(null, null);
                progressBar1.Value = i;
                Application.DoEvents();
            }
        }

        private void SaveToCSV(DataGridView DGV)
        {
            string filename = "";
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV (*.csv)|*.csv";
            sfd.FileName = "Output.csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Data will be exported and you will be notified when it is ready.");
                if (File.Exists(filename))
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                    }
                }
                int columnCount = DGV.ColumnCount;
                string columnNames = "";
                string[] output = new string[DGV.RowCount + 1];
                for (int i = 0; i < columnCount; i++)
                {
                    columnNames += DGV.Columns[i].Name.ToString() + ";";
                }
                output[0] += columnNames;
                for (int i = 1; (i - 1) < DGV.RowCount-1; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        output[i] += DGV.Rows[i - 1].Cells[j].Value.ToString() + ";";
                    }
                }
                System.IO.File.WriteAllLines(sfd.FileName, output, System.Text.Encoding.UTF8);
                MessageBox.Show("Your file was generated and its ready for use.");
            }
        }
        
        private void button7_Click(object sender, EventArgs e)
        {
            SaveToCSV(dataGridView1);
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        public static string UCS2ToString(string inText)
        {
            string res = "";
            if ((inText.Length == 0) || ((inText.Length % 2) != 0))
            {
                return null;
            }
            int num = inText.Length / 2;
            byte[] buffer = new byte[num];
            for (int i = 0; i < num; i++)
            {
                buffer[i] = byte.Parse(inText.Substring(i * 2, 2), NumberStyles.HexNumber);
            }
            res = Encoding.BigEndianUnicode.GetString(buffer);
            return res;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            MessageBox.Show(UCS2ToString("04110430043B0430043D0441003A002000320032002C0035000A041F043E043F043E043B043D04380442044C00200441044704350442003A000A0031003E041F043E043C043E0449044C002004340440044304330430000A0032003E04210020043A043004400442044B000A0033003E0050004C004100590042004F0059002E00310038002B002E00370434002F00300440"));
            MessageBox.Show(UCS2ToString("AA180C3602"));

        }
    }
}
