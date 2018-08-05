using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Configuration;
using MongoDB.Driver.GridFS;
using System.IO;
using System.Threading.Tasks;

namespace WebAppMongoDb.Models
{
    public class ComputerContext
    {
        IMongoDatabase database; // база данных
        IGridFSBucket gridFS;   // файловое хранилище

        public ComputerContext()
        {
            // строка подключения
            string connectionString = ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString;
            var connection = new MongoUrlBuilder(connectionString);
            // получаем клиента для взаимодействия с базой данных
            MongoClient client = new MongoClient(connectionString);
            // получаем доступ к самой базе данных
            database = client.GetDatabase(connection.DatabaseName);
            // получаем доступ к файловому хранилищу
            gridFS = new GridFSBucket(database);
        }
        // обращаемся к коллекции Phones
        public IMongoCollection<Computer> Computers
        {
            get { return database.GetCollection<Computer>("Computers"); }
        }
        // получаем все документы, используя критерии фильтрации
        public async Task<IEnumerable<Computer>> GetComputers(int? year, string name)
        {
            // строитель фильтров
            var builder = new FilterDefinitionBuilder<Computer>();
            var filter = builder.Empty; // фильтр для выборки всех документов
            // фильтр по имени
            if (!String.IsNullOrWhiteSpace(name))
            {
                filter = filter & builder.Regex("Name", new BsonRegularExpression(name));
            }
            if (year.HasValue)  // фильтр по году
            {
                filter = filter & builder.Eq("Year", year.Value);
            }

            return await Computers.Find(filter).ToListAsync();
        }

        // получаем один документ по id
        public async Task<Computer> GetComputer(string id)
        {
            return await Computers.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
        }
        // добавление документа
        public async Task Create(Computer c)
        {
            await Computers.InsertOneAsync(c);
        }
        // обновление документа
        public async Task Update(Computer c)
        {
            await Computers.ReplaceOneAsync(new BsonDocument("_id", new ObjectId(c.Id)), c);
        }
        // удаление документа
        public async Task Remove(string id)
        {
            await Computers.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));
        }
        // получение изображения
        public async Task<byte[]> GetImage(string id)
        {
            return await gridFS.DownloadAsBytesAsync(new ObjectId(id));
        }
        // сохранение изображения
        public async Task StoreImage(string id, Stream imageStream, string imageName)
        {
            Computer c = await GetComputer(id);
            if (c.HasImage())
            {
                // если ранее уже была прикреплена картинка, удаляем ее
                await gridFS.DeleteAsync(new ObjectId(c.ImageId));
            }
            // сохраняем изображение
            ObjectId imageId = await gridFS.UploadFromStreamAsync(imageName, imageStream);
            // обновляем данные по документу
            c.ImageId = imageId.ToString();
            var filter = Builders<Computer>.Filter.Eq("_id", new ObjectId(c.Id));
            var update = Builders<Computer>.Update.Set("ImageId", c.ImageId);
            await Computers.UpdateOneAsync(filter, update);
        }
    }
}