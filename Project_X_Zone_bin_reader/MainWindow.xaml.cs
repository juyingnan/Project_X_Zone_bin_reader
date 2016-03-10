using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project_X_Zone_bin_reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ItemEntries = new ObservableCollection<ItemEntry>();
            ItemDataGrid.DataContext = ItemEntries;
        }

        BinaryReader reader;
        string myString;
        ObservableCollection<ItemEntry> ItemEntries;

        private void OpenItemFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            StringBuilder sb = new StringBuilder();

            if (openFileDialog.ShowDialog() == true)
            {
                ItemFilePathTextBlock.Text = openFileDialog.FileName;
                FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open);
                int hexIn;
                String hex;
                for (int i = 0; (hexIn = fs.ReadByte()) != -1; i++)
                {
                    hex = string.Format("{0:X2}", hexIn);
                    sb.Append(hex);
                    sb.Append(" ");
                }
                fs.Close();
                ItemBinPreviewTextBox.Text = sb.ToString();
                reader = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.Open));
                myString = System.Text.Encoding.Unicode.GetString(reader.ReadBytes((int)reader.BaseStream.Length));
                reader.Close();
                ItemBinPreviewTextBox.Text = myString.Replace(myString[1], ' ');
            }

            var items = sb.ToString().Split(new string[] { "FF FC F0 03" }, StringSplitOptions.None);
            for (int i = 1; i < items.Length - 1; i++)
            {
                //sample
                //" 02 00 00 00 00 00 FB FF 00 00 00 00 46 00 00 00 0E 55 00 00 2E 55 00 00 "
                string nameString = Offset2HexString(items[i], 21, false);
                string desString = Offset2HexString(items[i], 9, false);
                int name = Convert.ToInt32(nameString, 16) / 2;
                int description = Convert.ToInt32(desString, 16) / 2;
                ItemEntry ie = new ItemEntry(myString, i.ToString(), name, description);
                // ie.atk = items[i + 1];
                string atkString = Offset2HexString(items[i], 16, true);
                string defString = Offset2HexString(items[i], 22, true);
                string spdString = Offset2HexString(items[i], 28, true);
                string tecString = Offset2HexString(items[i], 34, true);
                string hpString = Offset2HexString(items[i], 10, true);
                ie.atk = Convert.ToInt32(atkString, 16);
                ie.def = Convert.ToInt32(defString, 16);
                ie.spd = Convert.ToInt32(spdString, 16);
                ie.tec = Convert.ToInt32(tecString, 16);
                ie.hp = Convert.ToInt32(hpString, 16);
                ItemEntries.Add(ie);

                //int star
                //int start = items[i]
            }

            //StringBuilder sb = new StringBuilder();
            //ItemBinPreviewTextBox.Text = sb.ToString();

            //read as byte
            //byte[] bytes = reader.ReadBytes((int)reader.BaseStream.Length);
            //StringBuilder sb = new StringBuilder();
            //foreach (byte b in bytes)
            //    sb.Append(b);
            //ItemBinPreviewTextBox.Text = sb.ToString();
        }

        private static string Offset2HexString(string item, int offset, bool left2Right)
        {
            int l2r = left2Right ? 1 : -1;
            int nameHighOffset = (item.Length + l2r * offset) % item.Length;
            int nameLowOffset = (item.Length + l2r * offset - 3) % item.Length;
            string nameHigh = item.Substring(nameHighOffset, 2);
            string nameLow = item.Substring(nameLowOffset, 2);
            string nameString = nameHigh + nameLow;
            return nameString;
        }
    }
}

public class ItemEntry : INotifyPropertyChanged
{
    public ItemEntry(string str, string id, int nameOffset, int descriptionOffset)
    {
        this.id = id;
        this.name = GetStringFromHex(str, nameOffset);
        this.description = GetStringFromHex(str, descriptionOffset);
    }

    private string GetStringFromHex(string str, int start)
    {
        // char[] separator = System.Text.Encoding.Unicode.GetChars(new byte[0000]);
        char separator = '\0';
        string subStr = str.Substring(start - 1, 200);
        string[] strs = subStr.Split(separator);
        return strs[1];
    }

    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int tec { get; set; }
    public int spd { get; set; }
    public int hp { get; set; }
    public string sp { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
}