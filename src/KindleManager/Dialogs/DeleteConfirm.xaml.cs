﻿using System.Windows;

namespace KindleManager.Dialogs
{
    public partial class DeleteConfirm : DialogBase
    {
        public Visibility OnDevice { get; set; }
        public Visibility OnPC { get; set; }
        public Visibility OnBoth { get; set; }
        public string BookTitle { get; set; }
        public int DeleteFrom = -1;

        public DeleteConfirm(string bookTitle, bool onDevice, bool onPC)
        {
            this.DataContext = this;
            this.OnDevice = onDevice ? Visibility.Visible : Visibility.Collapsed;
            this.OnPC = onPC ? Visibility.Visible : Visibility.Collapsed;
            this.OnBoth = (onPC && onDevice) ? Visibility.Visible : Visibility.Collapsed;
            this.BookTitle = bookTitle;
            InitializeComponent();

            if (this.OnBoth != Visibility.Visible)
            {
                if (this.OnDevice == Visibility.Visible)
                {
                    this.cbDeleteFrom.SelectedIndex = 1;
                }
                else
                {
                    this.cbDeleteFrom.SelectedIndex = 2;
                }
            }
        }

        protected override void Confirm(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.DeleteFrom = cbDeleteFrom.SelectedIndex;
            this.Close(sender, e);
        }
    }
}
