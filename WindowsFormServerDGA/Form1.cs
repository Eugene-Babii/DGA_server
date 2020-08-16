using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsFormServerDGA.dbServerDGADataSet;


namespace WindowsFormServerDGA
{
    public partial class MainForm : Form
    {
        TcpListener listener = null;
        string path = String.Empty;

        SqlConnection conn = null;
        SqlCommand cmd = null;

        DGAResults dgaRes = null;                         //Объявляем объект класса DGAResult и инициализируем его null
        List<DGAResults> results = new List<DGAResults>(); //Объявляем список объектов lbooks типизированный классом DGAResult
        Transformer trans = null;
        List<Transformer> transformers = new List<Transformer>();

        double ch4 = 0, c2h4 = 0, c2h2 = 0;

        public MainForm()
        {
            string cs = ConfigurationManager.ConnectionStrings["WindowsFormServerDGA.Properties.Settings.dbServerDGAConnectionString"].ConnectionString;
            conn = new SqlConnection(cs);
            conn.Open();

            InitializeComponent();
        }
        private void btnShow_Click(object sender, EventArgs e)
        {
            DuvalsTriangle t = new DuvalsTriangle();
            t.drawZone(this);
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            DuvalsTriangle t = new DuvalsTriangle();
            t.triangleCleare(this);
        }
        private void btnListen_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;

            try
            {
                listener = new TcpListener(IPAddress.Parse(txtAddress.Text), Convert.ToInt32(txtPort.Text));
                Thread t = new Thread(ListenStart);
                t.IsBackground = true;
                t.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Server message");
            }
        }
        private void ListenStart()
        {

            listener.Start();
            ConnectToClient();
            btnListen.Invoke((MethodInvoker)(()=>
            {
                btnListen.BackColor = Color.Green;
                btnListen.Font = new Font(this.Font, FontStyle.Bold);
            }));
        }
        private void ConnectToClient()
        {
            listener.BeginAcceptTcpClient(new AsyncCallback(ClientConnected), listener);
        }
        private void ClientConnected(IAsyncResult ar)
        {
            TcpListener l = ar.AsyncState as TcpListener;
            l.BeginAcceptTcpClient(ClientConnected, l);

            try
            {
                TcpClient client = listener.EndAcceptTcpClient(ar);
                txtInfo.Invoke((MethodInvoker)(()=> txtInfo.Text += client.Client.RemoteEndPoint.ToString()+ "\r\n"));
                ReciveData(client);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Server message");
            }
        }
        private void ReciveData(object obj)
        {
            TcpClient client = obj as TcpClient;
            NetworkStream ns = client.GetStream();

            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                //transformers = (List<Transformer>)bf.Deserialize(ns);

                trans = (Transformer)bf.Deserialize(ns);

                string sqlCmd = "INSERT INTO ";
                string tblName = "[Transformers]";
                string tblColName = " (SerialNumber, DesignationId) VALUES (@SerialNumber, @DesignationId)";
                string addRecordTransformer = sqlCmd + tblName + tblColName;

                cmd = new SqlCommand(addRecordTransformer, conn);
                //cmd.Parameters.AddWithValue("Id", trans.Id);
                cmd.Parameters.AddWithValue("SerialNumber", trans.SerialNumber);
                cmd.Parameters.AddWithValue("DesignationId", trans.DesignationId);
                //this.tableAdapterManager.UpdateAll(this.dbServerDGADataSet);
                cmd.ExecuteNonQueryAsync();

                DataLoad();


                ns.Close();
                client.Close();

                //    //клиентская машинa - название папки
                //    string dirName = path + tag + "\\";

                //    if (!Directory.Exists(dirName))
                //    {
                //        Directory.CreateDirectory(dirName);
                //    }

                //    for (int i = 0; i < imgList.Count; i++)
                //    {
                //        string file_Name = dirName + tag + "_" + i.ToString() + ".jpg";

                //        //сохраняем картинку
                //        try
                //        {
                //            //BeginInvoke((MethodInvoker)(() => { imgList[i].Save(file_Name, ImageFormat.Jpeg); }));
                //            imgList[i].Save(file_Name, ImageFormat.Jpeg);
                //            Thread.Sleep(10);

                //        }
                //        catch (Exception ex)
                //        {
                //            MessageBox.Show(ex.Message);
                //        }

                //    }
                //    lstFiles.Invoke((MethodInvoker)(() => lstFiles.Items.Add(dirName)));
                //    FillListBox(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnClose_Resize(object sender, EventArgs e)
        {
            DuvalsTriangle t = new DuvalsTriangle();
            t.triangleCleare(this);
        }
        private void transformersBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.Validate();
                this.transformersBindingSource.EndEdit();
                this.dGAResultsBindingSource.EndEdit();
                this.tableAdapterManager.UpdateAll(this.dbServerDGADataSet);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            DataLoad();
        }
        private void DataLoad()
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "dbServerDGADataSet.DGAResults". При необходимости она может быть перемещена или удалена.
            this.dGAResultsTableAdapter.Fill(this.dbServerDGADataSet.DGAResults);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "dbServerDGADataSet.Transformers". При необходимости она может быть перемещена или удалена.
            this.transformersTableAdapter.Fill(this.dbServerDGADataSet.Transformers);

            lvDisolvedGasesLoad();
        }
        private void lvDisolvedGasesLoad()
        {
            try
            {
                SqlCommand comm = new SqlCommand();
                comm.Connection = conn;
                comm.CommandText = "select * from DGAResults";

                SqlDataReader reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    //Создаем объект типа DGAResult
                    dgaRes = new DGAResults();

                    //Инициализиурем свойства обекта данными из таблицы Books базы данны 
                    //из reader, после выполнения комманды "select * from Books"
                    dgaRes.Id = reader.GetInt32(reader.GetOrdinal("id"));
                    dgaRes.CH4 = reader.GetDecimal(reader.GetOrdinal("CH4"));
                    dgaRes.C2H4 = reader.GetDecimal(reader.GetOrdinal("C2H4"));
                    dgaRes.C2H2 = reader.GetDecimal(reader.GetOrdinal("C2H2"));


                    //Заполняем список объектов класса DGAResult
                    results.Add(dgaRes);

                }
                reader.Close();

                //Заносим значения в listView1
                int i = 0;
                foreach (var item in results)
                {
                    lvDisolvedGases.Items.Add((item.Id).ToString());
                    #region NonCorrect
                    //listView1.Items[item.ID - 1].SubItems.Add((item.AuthorID).ToString());
                    //listView1.Items[item.ID - 1].SubItems.Add((item.Title));
                    //listView1.Items[item.ID - 1].SubItems.Add((item.Price).ToString());
                    //listView1.Items[item.ID - 1].SubItems.Add((item.Pages).ToString());
                    #endregion
                    lvDisolvedGases.Items[i].SubItems.Add((item.Id).ToString());
                    lvDisolvedGases.Items[i].SubItems.Add((item.CH4).ToString()); ;
                    lvDisolvedGases.Items[i].SubItems.Add((item.C2H4).ToString());
                    lvDisolvedGases.Items[i].SubItems.Add((item.C2H2).ToString());
                    i += 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "dbServerDGADataSet.DGAResults". При необходимости она может быть перемещена или удалена.
            this.dGAResultsTableAdapter.Fill(this.dbServerDGADataSet.DGAResults);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "dbServerDGADataSet.Transformers". При необходимости она может быть перемещена или удалена.
            this.transformersTableAdapter.Fill(this.dbServerDGADataSet.Transformers);
        }
        private void lvDisolvedGases_SelectedIndexChanged(object sender, EventArgs e)
        {
            //decimal ch4 = (sender as DGAResultsRow).CH4;
            //decimal c2h4 = (sender as DGAResultsRow).C2H4;
            //decimal c2h2 = (sender as DGAResultsRow).C2H2;
        }
        private void transformersDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            DataGridError(sender, e);
        }
        private void dGAResultsDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            DataGridError(sender, e);
        }
        private void dGAResultsDataGridView_Enter(object sender, EventArgs e)
        {
            try
            {
                this.Validate();
                this.transformersBindingSource.EndEdit();
                this.tableAdapterManager.UpdateAll(this.dbServerDGADataSet);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void DataGridError(object sender, DataGridViewDataErrorEventArgs e)
        {
            //MessageBox.Show("Error happened " + e.Context.ToString());

            //if (e.Context == DataGridViewDataErrorContexts.Commit)
            if (e.Context.ToString().Contains(DataGridViewDataErrorContexts.Commit.ToString()))
            {
                MessageBox.Show("commit error");
            }

            //if (e.Context == DataGridViewDataErrorContexts.CurrentCellChange)
            if (e.Context.ToString().Contains(DataGridViewDataErrorContexts.CurrentCellChange.ToString()))
            {
                MessageBox.Show("cell change");
            }
            //if (e.Context == DataGridViewDataErrorContexts.Parsing)
            if (e.Context.ToString().Contains(DataGridViewDataErrorContexts.Parsing.ToString()))
            {
                MessageBox.Show("parsing error");
            }
            //if (e.Context == DataGridViewDataErrorContexts.LeaveControl)
            if (e.Context.ToString().Contains(DataGridViewDataErrorContexts.LeaveControl.ToString()))
            {
                MessageBox.Show("leave control error");
            }
            /*DataGridViewDataErrorContexts*/
            if ((e.Exception) is ConstraintException)
            {
                DataGridView view = (DataGridView)sender;
                view.Rows[e.RowIndex].ErrorText = "an error";
                view.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "an error";

                e.ThrowException = false;
            }

        }
        private void dGAResultsDataGridView1_Click(object sender, EventArgs e)
        {
            string txt_ch4 = dGAResultsDataGridView1.CurrentRow.Cells[6].Value.ToString();
            string txt_c2h4 = dGAResultsDataGridView1.CurrentRow.Cells[8].Value.ToString();
            string txt_c2h2 = dGAResultsDataGridView1.CurrentRow.Cells[9].Value.ToString();
            //MessageBox.Show($"ch4 = {txt_ch4}, c2h4 =  {txt_c2h4}, c2h2 =  {txt_c2h2}");

            ch4 = double.Parse(txt_ch4);
            c2h4 = double.Parse(txt_c2h4);
            c2h2 = double.Parse(txt_c2h2);

            DuvalsTriangle t = new DuvalsTriangle();
            t.getLocation(ch4, c2h4, c2h2);
            t.drawZone(this);

        }


    }
}
