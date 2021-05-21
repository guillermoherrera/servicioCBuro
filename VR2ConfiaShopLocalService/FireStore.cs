using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using Grpc.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace estadosCBService
{
    class FireStore
    {
        public static FirestoreDb conexionDB()
        {
            var credential = GoogleCredential.FromFile("C:\\Users\\gherr\\Downloads\\SGCC-d42386a165af.json");
            Channel channel = new Channel(FirestoreClient.DefaultEndpoint.Host, FirestoreClient.DefaultEndpoint.Port, credential.ToChannelCredentials());
            FirestoreClient client = FirestoreClient.Create(channel);

            FirestoreDb db = FirestoreDb.Create("sgcc-57fde", client);
            return db;
        }

        public static async Task<List<Solicitud>> GetSolicitudesFromFireStore()
        {
            
            List<Solicitud> solicitudes = new List<Solicitud>();
            try
            {
                FirestoreDb db = conexionDB();

                Query capitalQuery = db.Collection("Solicitudes").WhereEqualTo("status", 7);
                QuerySnapshot capitalQuerySnapshot = await capitalQuery.GetSnapshotAsync();
                foreach (DocumentSnapshot document in capitalQuerySnapshot.Documents)
                {
                    Solicitud solicitud = document.ConvertTo<Solicitud>();
                    solicitud.solicitudID = document.Id;
                    solicitudes.Add(solicitud);
                }
                //Renovaciones
                Query capitalQuery2 = db.Collection("Renovaciones").WhereEqualTo("status", 7);
                QuerySnapshot capitalQuerySnapshot2 = await capitalQuery2.GetSnapshotAsync();
                foreach (DocumentSnapshot document in capitalQuerySnapshot2.Documents)
                {
                    Solicitud solicitud = document.ConvertTo<Solicitud>();
                    solicitud.solicitudID = document.Id;
                    solicitudes.Add(solicitud);
                }
            }
            catch (Exception ex)
            {
                Log.Information("*****Error Exception GetSolicitudesFromFireStore: {0}", ex.Message);
            }
            return solicitudes;
        }

        public static async Task<bool> ActualizaStatusConsulta(string _idXRB, string mensaje, int status, String url)
        {
            bool result = false;
            try
            {
                FirestoreDb db = conexionDB();
                Query capitalQuery = db.Collection("Solicitudes").WhereEqualTo("CB_idXRB", int.Parse(_idXRB));
                QuerySnapshot capitalQuerySnapshot = await capitalQuery.GetSnapshotAsync();
                DocumentReference solicitudAux = null;
                Dictionary<string, object> updates = new Dictionary<string, object>();
                await db.RunTransactionAsync(async transaction =>
                {
                    
                    foreach (DocumentSnapshot document in capitalQuerySnapshot.Documents)
                    {
                        solicitudAux = db.Collection("Solicitudes").Document(document.Id);
                        updates = new Dictionary<string, object>{
                            {"status", status }
                        }; 
                    }

                    if (capitalQuerySnapshot.Documents.Count == 0)
                    {
                        Query capitalQuery2 = db.Collection("Renovaciones").WhereEqualTo("CB_idXRB", _idXRB);
                        QuerySnapshot capitalQuerySnapshot2 = await capitalQuery2.GetSnapshotAsync();
                        foreach (DocumentSnapshot document in capitalQuerySnapshot2.Documents)
                        {
                            solicitudAux = db.Collection("Renovaciones").Document(document.Id);
                            updates = new Dictionary<string, object>{
                            {"status", status }
                        };
                        }
                    }
                    
                    if (status == 9) { updates.Add("documentoBuroPdf", url); }
                    if (status == 10) { updates.Add("mensajeErrorConsultaBuro", mensaje); }
                    transaction.Update(solicitudAux, updates);
                });
                result = true;
            }
            catch (Exception ex)
            {
                Log.Information("*****Error Exception ActualizaStatusConsulta: {0}", ex.Message);
                result = false;
            }
            return result;
        }
    }
}
