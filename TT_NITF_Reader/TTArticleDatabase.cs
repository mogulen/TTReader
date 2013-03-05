using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace TTNITFReader
{
    public delegate void DBErrorEventHandler(Exception Ex);
    public class TTArticleDatabase
    {

        
        
        //Connect to the database for Collections Table
        //public SqlConnection Connection = new SqlConnection(@"Server=z27ar1cs01vs02\b301430;connection timeout=30;Database=TV4_Text;user id=texttv_app;pwd=d27ftt");
        public SqlConnection Connection = new SqlConnection();

        public event DBErrorEventHandler OnError;

        public TTArticleDatabase()
        {
        }
        public TTArticleDatabase(String ConnectionString)
        {
            Connection = new SqlConnection(ConnectionString);
        }
        public TTArticleDatabase(SqlConnection Conn)
        {
            Connection = Conn;           
        }
        public bool Connect()
        {
            bool ret = true;
            try
            {
                Connection.Open();
            }
            catch (Exception ex)
            {
                ret = false;
                //Console.WriteLine("Error: " + ex.Message);
                CallOnError(ex);
            }

            return ret;
        }
        public void Disconnect()
        {
            Connection.Close();
        }
        
        public void Add(TTNITFReader.TTData Data)
        {
            try
            {

                //foreach (string RefId in Data.RefId)
                //{


                    SqlCommand Cmd = new SqlCommand("insert into TV4_text.dbo.TT_Article (Id, RefId, SendTime, Embargo, Prio, Action, Status, FixId, Type, Source, Headline, HeadlineShort, TextShort, HeadlineLong, TextLong, Cat_inr, Cat_utr, Cat_eko, Cat_it, Cat_spt, Cat_bng, Cat_frc, Cat_klt, Cat_mtr, Cat_mde, Cat_noj, Cat_spel, Nr, RefNr) values(@Id, @RefId, @SendTime, @Embargo, @Prio, @Action, @Status, @FixId, @Type, @Source, @Headline, @HeadlineShort, @TextShort, @HeadlineLong, @TextLong, @Cat_inr, @Cat_utr, @Cat_eko, @Cat_it, @Cat_spt, @Cat_bng, @Cat_frc, @Cat_klt, @Cat_mtr, @Cat_mde, @Cat_noj, @Cat_spel, @Nr, @RefNr)", Connection);

                    Cmd.Parameters.Add(ToParameter("@Id", 80, Data.Id));
                    //Cmd.Parameters.Add(ToParameter("@RefId", 80, Data.RefId));

                    if (Data.RefId != null)
                    {
                        if (Data.RefId.Count() == 0)
                            Cmd.Parameters.Add(ToParameter("@RefId", 80, ""));
                        else
                            Cmd.Parameters.Add(ToParameter("@RefId", 80, Data.RefId[0]));
                    }
                    else
                        Cmd.Parameters.Add(ToParameter("@RefId", 80, ""));
                    
                    Cmd.Parameters.Add(ToParameter("@SendTime", Data.SendDate));
                    Cmd.Parameters.Add(ToParameter("@Embargo", Data.Embargo));
                    Cmd.Parameters.Add(ToParameter("@Prio", Data.Prio));
                    Cmd.Parameters.Add(ToParameter("@Action", Data.Action));

                    if ((Data.Status.Length == 0) && (Data.ShortArticle.RefId[0] != 0))
                        Cmd.Parameters.Add(ToParameter("@Status", 54, "Ersätter"));
                    else
                        Cmd.Parameters.Add(ToParameter("@Status", 54, Data.Status));
                    
                    Cmd.Parameters.Add(ToParameter("@FixId", 3, Data.FixId));
                    Cmd.Parameters.Add(ToParameter("@Type", Data.Type));
                    //Cmd.Parameters.Add(ToParameter("@Source", 32, Data.HSource));
                    if (Data.LongArticle.Source.Length > 0)
                        Cmd.Parameters.Add(ToParameter("@Source", 32, Data.LongArticle.Source));
                    else
                        Cmd.Parameters.Add(ToParameter("@Source", 32, Data.HSource));

                    if (Data.Headline.Length > 0)
                        Cmd.Parameters.Add(ToParameter("@Headline", 60, Data.Headline));
                    else
                        Cmd.Parameters.Add(ToParameter("@Headline", 60, Data.ShortArticle.Headline));

                    Cmd.Parameters.Add(ToParameter("@HeadlineShort", 40, Data.ShortArticle.Headline));
                    if (Data.ShortArticle.Text != null)
                    {
                        String Text = "";

                        foreach (String txt in Data.ShortArticle.Text)
                            Text += txt + "\r\n";

                        Cmd.Parameters.Add(ToParameter("@TextShort", Text.Length, Text.Trim()));
                    }
                    else
                        Cmd.Parameters.Add(ToParameter("@TextShort", 1, ""));

                    Cmd.Parameters.Add(ToParameter("@HeadlineLong", Data.LongArticle.Headline));
                    if (Data.LongArticle.Text != null)
                    {
                        String Text = "";

                        foreach (String txt in Data.LongArticle.Text)
                            Text += txt + "\r\n";

                        Cmd.Parameters.Add(ToParameter("@TextLong", Text.Length, Text.Trim()));
                    }
                    else
                        Cmd.Parameters.Add(ToParameter("@TextLong", 1, ""));

                    Cmd.Parameters.Add(ToParameter("@Cat_inr", Data.ShortArticle.Category[(int)TTNITFReader.TTShortArticle.CategoryType.Inrikes]));
                    Cmd.Parameters.Add(ToParameter("@Cat_utr", Data.ShortArticle.Category[(int)TTNITFReader.TTShortArticle.CategoryType.Utrikes]));
                    Cmd.Parameters.Add(ToParameter("@Cat_eko", Data.ShortArticle.Category[(int)TTNITFReader.TTShortArticle.CategoryType.Ekonomi]));
                    Cmd.Parameters.Add(ToParameter("@Cat_it", Data.ShortArticle.Category[(int)TTNITFReader.TTShortArticle.CategoryType.IT]));
                    Cmd.Parameters.Add(ToParameter("@Cat_spt", Data.ShortArticle.Category[(int)TTNITFReader.TTShortArticle.CategoryType.Sport]));
                    Cmd.Parameters.Add(ToParameter("@Cat_bng", Data.ShortArticle.Category[(int)TTNITFReader.TTShortArticle.CategoryType.Kändis]));
                    Cmd.Parameters.Add(ToParameter("@Cat_frc", Data.ShortArticle.Category[(int)TTNITFReader.TTShortArticle.CategoryType.Film]));
                    Cmd.Parameters.Add(ToParameter("@Cat_klt", Data.ShortArticle.Category[(int)TTNITFReader.TTShortArticle.CategoryType.Kultur]));
                    Cmd.Parameters.Add(ToParameter("@Cat_mtr", Data.ShortArticle.Category[(int)TTNITFReader.TTShortArticle.CategoryType.Motor]));
                    Cmd.Parameters.Add(ToParameter("@Cat_mde", Data.ShortArticle.Category[(int)TTNITFReader.TTShortArticle.CategoryType.Mode]));
                    Cmd.Parameters.Add(ToParameter("@Cat_noj", Data.ShortArticle.Category[(int)TTNITFReader.TTShortArticle.CategoryType.Nöje]));
                    Cmd.Parameters.Add(ToParameter("@Cat_spel", Data.ShortArticle.Category[(int)TTNITFReader.TTShortArticle.CategoryType.Spel]));

                    /*
                    String temp = Data.SendDate.ToString("yyyyMMdd");
                    Cmd.Parameters.Add(ToParameter("@Nr", 16,temp + ":" + Data.ShortArticle.Id.ToString("D4")));
                    */

                    Cmd.Parameters.Add(ToParameter("@Nr", 16, Data.ShortArticle.Id.ToString("D4")));


                    if (Data.ShortArticle.RefId[0] != 0)
                    {
                        //Cmd.Parameters.Add(ToParameter("@RefNr", 16, temp + ":" + Data.ShortArticle.RefId.ToString("D4")));
                        Cmd.Parameters.Add(ToParameter("@RefNr", 64, Data.ShortArticle.RefId[0].ToString("D4")));

                        /*
                        String RefId = "";
                        foreach (int n in Data.ShortArticle.RefId)
                            RefId += n.ToString("D4") + " ";

                        RefId = RefId.Trim();

                        Cmd.Parameters.Add(ToParameter("@RefNr", 64, RefId));
                        */

                    }
                    else
                        Cmd.Parameters.Add(ToParameter("@RefNr", 16, ""));

                    int resp = Cmd.ExecuteNonQuery();
                //}
            }
            catch (Exception ex)
            {
                CallOnError(ex);
                //Console.WriteLine("Error: " + ex.Message);
            }

        }
        private SqlParameter ToParameter(String Column, String Value)
        {
            SqlParameter Ret = new SqlParameter(Column, System.Data.SqlDbType.Text);
            Ret.Value = Value;

            return Ret;

        }
        private SqlParameter ToParameter(String Column, int Size, String Value)
        {
            SqlParameter Ret = new SqlParameter(Column, System.Data.SqlDbType.VarChar, Size);
            Ret.Value = Value;

            return Ret;

        }

        private SqlParameter ToParameter(String Column, int Value)
        {
            SqlParameter Ret = new SqlParameter(Column, System.Data.SqlDbType.Int);
            Ret.Value = Value;

            return Ret;
        }
        private SqlParameter ToParameter(String Column, DateTime Value)
        {
            SqlParameter Ret = new SqlParameter(Column, System.Data.SqlDbType.DateTime);
            Ret.Value = Value;

            return Ret;
        }
        private void CallOnError(Exception ex)
        {
            if (OnError != null)
                OnError(ex);
        }
    }
}
