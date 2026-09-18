using Microsoft.Extensions.Configuration;
using Supabase.Interfaces;
using Supabase;              // para Supabase.Client
using Supabase.Storage;      // para StorageBucketClient y FileOptions

// Alias para evitar ambigüedad
using SupabaseClient = Supabase.Client;

namespace CentroSenderos_2026_Servicio
{
    public class SupabaseStorageService
    {
        private readonly SupabaseClient client;
        private readonly string bucket;

        public SupabaseStorageService(IConfiguration config)
        {
            var url = config["Supabase:Url"] ?? "";
            var key = config["Supabase:ServiceRoleKey"] ?? "";
            bucket = config["Supabase:Bucket"] ?? "";

            client = new SupabaseClient(url, key);
        }

        public async Task<string> SubirDocumentoAsync(Stream fileStream, string fileName, string contentType)
        {
            await client.InitializeAsync();

            var storage = client.Storage.From(bucket);

            // Convertir Stream a byte[]
            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            // Upload devuelve un string (la ruta del archivo)
            var result = await storage.Upload(bytes, fileName, new Supabase.Storage.FileOptions
            {
                ContentType = contentType,
                Upsert = true
            });

            return result; // ya es un string, no tiene FullPath
        }


        public async Task<bool> EliminarDocumentoAsync(string fileName)
        {
            await client.InitializeAsync();

            var storage = client.Storage.From(bucket);
            await storage.Remove(fileName);
            return true;
        }
    }
}

