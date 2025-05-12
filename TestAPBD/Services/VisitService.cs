using System.Data.Common;
using Microsoft.Data.SqlClient;
using TestAPBD.Exceptions;
using TestAPBD.Models.DTOs;

namespace TestAPBD.Services;

public class VisitService : IVisitService
{
    private readonly IConfiguration _configuration;

    public VisitService(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public async Task<bool> VisitExistsAsync(int id)
    {
        await using SqlConnection connection =
            new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await using SqlCommand command = new SqlCommand();

        string sql = "select count(*) from Visit where visit_id = @id";
        command.Connection = connection;
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@id", id));

        await connection.OpenAsync();


        return !((int)await command.ExecuteScalarAsync() == 0);
    }


    public async Task<bool> ClientExistsAsync(int id)
    {
        await using SqlConnection connection =
            new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await using SqlCommand command = new SqlCommand();

        string sql = "select count(*) from Client where client_id = @id";
        command.Connection = connection;
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@id", id));

        await connection.OpenAsync();


        return !((int)await command.ExecuteScalarAsync() == 0);
    }


    public async Task<bool> MechanicExistsAsync(string licence)
    {
        await using SqlConnection connection =
            new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await using SqlCommand command = new SqlCommand();

        string sql = "select count(*) from Mechanic where licence_number = @licence";
        command.Connection = connection;
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@licence", licence));

        await connection.OpenAsync();


        return !((int)await command.ExecuteScalarAsync() == 0);
    }
    
    public async Task<bool> ServiceExistsAsync(string name)
    {
        await using SqlConnection connection =
            new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await using SqlCommand command = new SqlCommand();

        string sql = "select count(*) from Service where name = @name";
        command.Connection = connection;
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@name", name));

        await connection.OpenAsync();


        return !((int)await command.ExecuteScalarAsync() == 0);
    }


    public async Task<VisitDetailsDto> GetVisitAsync(int visitId)
    {
        await using SqlConnection connection =
            new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();

        if (!await VisitExistsAsync(visitId))
        {
            throw new NotFoundException($"Visit {visitId} not found");
        }

        VisitDetailsDto visit = null;

        command.CommandText = @"select v.date, c.first_name, c.last_name,
                                c.date_of_birth, m.mechanic_id, m.licence_number,
                                s.name, vs.service_fee
                                from Visit v 
          join Mechanic m on m.mechanic_id = v.mechanic_id 
          join Client c   on  c.client_id = v.client_id 
          join visit_service vs on vs.visit_id=v.visit_id
          join service s on s.service_id = vs.service_id 
          where v.visit_id = @id";

        command.Parameters.Add(new SqlParameter("@id", visitId));

        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                if (visit == null)
                {
                    visit = new VisitDetailsDto
                    {
                        Date = reader.GetDateTime(0),
                        Client = new ClientDto
                        {
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            DateOfBirth = reader.GetDateTime(3),
                        },
                        Mechanic = new MechanicDto
                        {
                            MechanicId = reader.GetInt32(4),
                            LicenceNumber = reader.GetString(5),
                        },
                        VisitServices = new List<VisitServiceDto>()
                    };
                }

                visit.VisitServices.Add(new VisitServiceDto
                {
                    Name = reader.GetString(6),
                    ServiceFee = reader.GetDecimal(7)
                });
            }


            return visit;
        }
    }

    public async Task CreateVisitAsync(CreateVisitDto visit)
    {
        await using SqlConnection connection =
            new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        
        try
        {
            if (await VisitExistsAsync(visit.VisitId))
            {
                throw new ConflictException($"Visit {visit.VisitId} already exists");
            }


            if (!await MechanicExistsAsync(visit.MechanicLicenceNumber))
            {
                throw new NotFoundException($"Mechanic {visit.MechanicLicenceNumber} not found");
            }

            if (!await ClientExistsAsync(visit.ClientId))
            {
                throw new NotFoundException($"Client {visit.ClientId} not found");
            }

            foreach (var service in visit.Services)
            {
                if (!await ServiceExistsAsync(service.ServiceName))
                {
                    throw new NotFoundException($"Service {service.ServiceName} not found");
                }
            }
            
            

            command.CommandText = @"select mechanic_id from Mechanic where licence_number = @licence";
            command.Parameters.AddWithValue("@licence", visit.MechanicLicenceNumber);
            var mechanicId = (int)await command.ExecuteScalarAsync();

            command.Parameters.Clear();
            command.CommandText = @"select client_id from Client where client_id = @clientId";
            command.Parameters.AddWithValue("@clientId", visit.ClientId);
            var clientId = (int)await command.ExecuteScalarAsync();


            command.Parameters.Clear();
            command.CommandText = @"Insert into Visit 
                  Values (@visitId, @clientId, @mechanicId, GETDATE())";
            command.Parameters.AddWithValue("@visitId", visit.VisitId);
            command.Parameters.AddWithValue("@clientId", clientId);
            command.Parameters.AddWithValue("@mechanicId", mechanicId);
            await command.ExecuteNonQueryAsync();

            foreach (var service in visit.Services)
            {
                command.Parameters.Clear();
                command.CommandText = @"select service_id from Service where name = @serviceName";
                command.Parameters.AddWithValue("@serviceName", service.ServiceName);
                var serviceId = (int)await command.ExecuteScalarAsync();

                command.Parameters.Clear();
                command.CommandText = @"Insert into Visit_Service
                    Values(@VisitId, @serviceId, @ServiceFee)";
                command.Parameters.AddWithValue("@VisitId", visit.VisitId);
                command.Parameters.AddWithValue("@serviceId", serviceId);
                command.Parameters.AddWithValue("@ServiceFee", service.ServiceFee);
                await command.ExecuteNonQueryAsync();
            }


            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw ex;
        }
    }
}