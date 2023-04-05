using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GenerateDorukReport
{
    public partial class ReportForm : Form
    {
        public ReportForm()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            var jsonFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles");
            var orders = JsonFileReader.Read<List<Order>>(Path.Combine(jsonFilesPath, "orders.json"));
            var stopovers = JsonFileReader.Read<List<Stopover>>(Path.Combine(jsonFilesPath, "stopovers.json"));

            var reportItems = new Dictionary<int, Dictionary<string, int>>();
            var reasons = new List<string>();
            foreach (var stopover in stopovers)
            {
                if (!reasons.Contains(stopover.Reason))
                    reasons.Add(stopover.Reason);

                var matchedOrders = orders.Where(o => stopover.Start >= o.Start && stopover.Start <= o.End)
                    //.Where(m => stopover.End >= m.Start && stopover.End <= m.End)
                    .ToArray();

                foreach (var m in matchedOrders)
                {
                    var calcEndDate = stopover.End;

                    if (m.End < stopover.End)
                        calcEndDate = m.End;

                    var totalStopoverMinutes = (int)calcEndDate.Subtract(stopover.Start).TotalMinutes;

                    if (reportItems.ContainsKey(m.Id))
                    {
                        var items = reportItems[m.Id];
                        var existsData = items.FirstOrDefault(i => i.Key == stopover.Reason);
                        if(!existsData.Equals(default(KeyValuePair<string, int>)))
                        {
                            items.Remove(existsData.Key);
                            items.Add(stopover.Reason, existsData.Value + totalStopoverMinutes);
                            reportItems[m.Id] = items;
                            continue;
                        }

                        reportItems[m.Id].Add(stopover.Reason, totalStopoverMinutes);
                        continue;
                    }

                    reportItems.Add(m.Id, new Dictionary<string, int>() {
                        { stopover.Reason, totalStopoverMinutes }
                    });
                }  
            }
            
            var dt = new DataTable();
            var columns = reasons.ToList();

            //Birden fazla kullanıldığı için değişkene aldım.
            var orderFieldName = "İş Emri";
            var totalFieldName = "Toplam";

            columns.Insert(0, orderFieldName);
            columns.Add(totalFieldName);
            
            foreach(var col in columns)
                dt.Columns.Add(col);

            //Herhangi bir duruşa girmemiş iş emirlerini ekleyelim, sonrasında sıralayacağız
            var freeOrders = orders.Select(o => o.Id).ToList().Except(reportItems.Select(rp => rp.Key).ToList()).ToList();

            //Boş olarak listeye eklenmesi için
            foreach (var fo in freeOrders)
                reportItems.Add(fo, new Dictionary<string, int>() { });

            foreach (var item in reportItems)
            {
                var row = dt.NewRow();
                foreach (var col in columns)
                    row[col] = "...";

                row[0] = item.Key;
                var total = 0;
                foreach (var reason in item.Value)
                {
                    row[reason.Key] = reason.Value;
                    total += reason.Value;
                }

                row[totalFieldName] = total;
                dt.Rows.Add(row);
            }

            //Son satıra eklenmesi gereken toplam değerleri için
            var totalRow = dt.NewRow();
            totalRow[orderFieldName] = totalFieldName;
            foreach (var reason in reasons)
                totalRow[reason] = dt.AsEnumerable().Sum(r => r[reason].ToString() == "..." ? 0 : int.Parse(r[reason].ToString()));

            //Eğer ... var ise sıfır olarak alalım
            totalRow[totalFieldName] = dt.AsEnumerable().Sum(r => r[totalFieldName].ToString() == "..." ? 0 : int.Parse(r[totalFieldName].ToString()));
            dt.Rows.Add(totalRow);

            grdData.DataSource = dt;

            //Karışık eklediğimiz iş emirlerini sıralayalım
            grdData.Sort(grdData.Columns[orderFieldName], System.ComponentModel.ListSortDirection.Ascending);
        }
    }
}
