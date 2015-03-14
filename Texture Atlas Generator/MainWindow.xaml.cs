using System;
using System.Collections.Generic;
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
using System.Windows.Markup;
using System.IO;
using System.Xml;

namespace Texture_Atlas_Generator
{
    [global::System.ComponentModel.TypeConverter(typeof(SpriteInfoConverter))]
    public class SpriteInfo
    {
        public string Name { get; set; }
        public int AnimationGroup { get; set; }

        public SpriteInfo()
        {
        }

        public SpriteInfo(string name, int animationgroup)
        {
            this.Name = name;
            this.AnimationGroup = animationgroup;
        }

        public static SpriteInfo Parse(string data)
        {
            if (string.IsNullOrEmpty(data)) return new SpriteInfo();

            string[] items = data.Split(',');
            if (items.Count() != 2)
                throw new FormatException("SpriteInfo should have both Name and Animation Group");

            string name;
            int animationgroup;
            try
            {
                name = items[0].ToString();
            }
            catch (Exception ex)
            {
                throw new FormatException("Name value cannot be converted", ex);
            }

            try
            {
                animationgroup = Convert.ToInt32(items[1]);
            }
            catch (Exception ex)
            {
                throw new FormatException("Animation Group value cannot be converted", ex);
            }

            return new SpriteInfo(name, animationgroup);
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", this.Name, this.AnimationGroup);
        }
    }

    public class SpriteInfoConverter : global::System.ComponentModel.TypeConverter
    {

        //should return true if sourcetype is string
        public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType is string)
                return true;
            return base.CanConvertFrom(context, sourceType);
        }
        //should return true when destinationtype if SpriteInfo
        public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType is string)
                return true;

            return base.CanConvertTo(context, destinationType);
        }
        //Actual convertion from string to SpriteInfo
        public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    return SpriteInfo.Parse(value as string);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Cannot convert '{0}' ({1}) because {2}", value, value.GetType(), ex.Message), ex);
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        //Actual convertion from SpriteInfo to string
        public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            SpriteInfo spriteinfo = value as SpriteInfo;

            if (spriteinfo != null)
                if (this.CanConvertTo(context, destinationType))
                    return spriteinfo.ToString();

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Rectangle selectedElement;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FileMenu_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void FileMenu_OpenSS_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.Filter = "png files (*.png)|*.png|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                canvas_SpriteSheet.Children.Clear();
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(filename);
                image.EndInit();
                image_SpriteSheet.Source = image;
                canvas_SpriteSheet.Width = image.Width;
                canvas_SpriteSheet.Height = image.Height;
                canvas_SpriteSheet.Children.Add(image_SpriteSheet);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void buttonClick_DisplayGrid(object sender, RoutedEventArgs e)
        {
            canvas_SpriteSheet.Children.RemoveRange(1, canvas_SpriteSheet.Children.Count);

            int spriteWidth = Convert.ToInt32(textBox_SpriteWidth.Text);
            int spriteHeight = Convert.ToInt32(textBox_SpriteHeight.Text);
            int paddingWidth = Convert.ToInt32(textBox_PaddingWidth.Text);
            int paddingHeight = Convert.ToInt32(textBox_PaddingHeight.Text);
            int leftOffset = Convert.ToInt32(textBox_LeftOffset.Text);
            int topOffset = Convert.ToInt32(textBox_TopOffset.Text);

            for (int y = 0; y < (canvas_SpriteSheet.Height / (spriteHeight + paddingHeight + topOffset)) + 2; y++)
            {
                for (int x = 0; x < (canvas_SpriteSheet.Width / (spriteWidth + paddingWidth + leftOffset)) + 1; x++)
                {
                    SpriteInfo spriteTag = new SpriteInfo("Moo " + (x*y), x+y);
                    Rectangle rectangle = new Rectangle();
                    rectangle.Width = spriteWidth;
                    rectangle.Height = spriteHeight;
                    rectangle.Fill = Brushes.Red;
                    rectangle.Opacity = 0.1;
                    rectangle.Tag = spriteTag;
                    Canvas.SetTop(rectangle, topOffset + (paddingHeight * y) + (spriteHeight * y));
                    Canvas.SetLeft(rectangle, leftOffset + (paddingWidth * x) + (spriteWidth * x));
                    canvas_SpriteSheet.Children.Add(rectangle);
                }
            }
        }

        private void SpriteSheetGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var rectangles = canvas_SpriteSheet.Children.OfType<Rectangle>();

            if (selectedElement != null)
            {
                selectedElement.Fill = Brushes.Red;
                selectedElement = null;
                textBox_SelectedName.Text = "";
            }

            foreach (Rectangle rect in rectangles)
            {
                if(rect.IsMouseOver)
                {
                    selectedElement = rect;
                    selectedElement.Fill = Brushes.Green;
                    SpriteInfo info = (SpriteInfo)rect.Tag;
                    textBox_SelectedName.Text = info.Name;
                    break;
                }
            }
        }

        private void buttonClick_DeleteSelected(object sender, RoutedEventArgs e)
        {
            canvas_SpriteSheet.Children.Remove(selectedElement);
            textBox_SelectedName.Text = "";
            selectedElement = null;
        }

        private void FileMenu_ExportTA_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();

            saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(saveFileDialog.FileName))
                {
                    var rectangles = canvas_SpriteSheet.Children.OfType<Rectangle>();

                    foreach (Rectangle rect in rectangles)
                    {
                        SpriteInfo info = (SpriteInfo)rect.Tag;
                        string line;
                        //line += canvas_SpriteSheet.Children.IndexOf(rect).ToString();
                        line = info.Name
                             + " = "
                             + Canvas.GetLeft(rect).ToString()
                             + " "
                             + Canvas.GetTop(rect).ToString()
                             + " "
                             + textBox_SpriteWidth.Text
                             + " "
                             + textBox_SpriteHeight.Text;
                        file.WriteLine(line);
                    }
                }
            }
        }

        private void selectedName_KeyUp(object sender, KeyEventArgs e)
        {
            SpriteInfo info = (SpriteInfo)selectedElement.Tag;
            info.Name = textBox_SelectedName.Text;
        }

        private void FileMenu_SaveProject_Click(object sender, RoutedEventArgs e)
        {
            if(selectedElement != null)
            {
                selectedElement.Fill = Brushes.Red;
                selectedElement = null;
                textBox_SelectedName.Text = "";
            }

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();

            saveFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string mystrXAML = XamlWriter.Save(canvas_SpriteSheet);
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(saveFileDialog.FileName))
                {
                    file.WriteLine(mystrXAML);
                }
            }
        }

        private void FileMenu_LoadProject_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string inputData;
                spriteGrid.Children.Remove(canvas_SpriteSheet);
                using (StreamReader sr = new StreamReader(openFileDialog.FileName))
                {
                    inputData = sr.ReadToEnd();
                }
                StringReader stringReader = new StringReader(inputData);
                XmlReader xmlReader = XmlReader.Create(stringReader);
                canvas_SpriteSheet.Children.Clear();
                canvas_SpriteSheet = (Canvas)XamlReader.Load(xmlReader);
                spriteGrid.Children.Add(canvas_SpriteSheet);
                //canvas_SpriteSheet.MouseLeftButtonDown += new MouseButtonEventHandler(SpriteSheetGrid_MouseLeftButtonDown);
            }
         }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && (selectedElement != null))
            {
                canvas_SpriteSheet.Children.Remove(selectedElement);
                textBox_SelectedName.Text = "";
                selectedElement = null;
            }
        }
    }
}
