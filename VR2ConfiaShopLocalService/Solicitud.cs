using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Text;

namespace estadosCBService
{
    [FirestoreData]
    class Solicitud
    {
        public string solicitudID { get; set; }
        [FirestoreProperty]
        public int CB_idXRB { get; set; }
    }
}
