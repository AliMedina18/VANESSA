using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vanessa.Data;
using Microsoft.EntityFrameworkCore;

namespace Vanessa.Services
{
    public class EliminarUsuariosInactivosService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public EliminarUsuariosInactivosService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await EliminarUsuariosInactivos();
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Corre una vez al día
            }
        }

        private async Task EliminarUsuariosInactivos()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var fechaLimite = DateTime.Now.AddDays(-30);

                var usuariosParaEliminar = await context.Usuarios
                    .Where(u => !u.Activo && u.FechaInactivo <= fechaLimite)
                    .ToListAsync();

                if (usuariosParaEliminar.Any())
                {
                    context.Usuarios.RemoveRange(usuariosParaEliminar);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
