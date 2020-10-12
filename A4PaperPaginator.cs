using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace BarcodeAppDesktop.Helper
{
    class A4PaperPaginator : DocumentPaginator
    {
        public override DocumentPage GetPage(int pageNumber)
        {
            throw new NotImplementedException();
        }

        public override bool IsPageCountValid
        {
            get { throw new NotImplementedException(); }
        }

        public override int PageCount
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Windows.Size PageSize
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override IDocumentPaginatorSource Source
        {
            get { throw new NotImplementedException(); }
        }
    }
}
