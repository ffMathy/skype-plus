using SkypePlus.DataAccess;
using SkypePlus.Models;
using System.Collections.Generic;

namespace SkypePlus
{
    public partial class MainWindow
    {

        public MainWindow()
        {
            var access = SkypeDataAccess.Instance;
            access.NewMessagesArrived += Access_NewMessagesArrived;

            InitializeComponent();
        }

        private void Access_NewMessagesArrived(IEnumerable<Message> collection)
        {
            foreach (var message in collection)
            {
                
            }
        }
    }
}
