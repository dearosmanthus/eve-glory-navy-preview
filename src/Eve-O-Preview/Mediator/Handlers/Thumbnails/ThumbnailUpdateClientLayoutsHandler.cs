using System.Threading;
using System.Threading.Tasks;
using EveOPreview.Mediator.Messages;
using EveOPreview.Services;
using MediatR;

namespace EveOPreview.Mediator.Handlers.Thumbnails
{
	sealed class ThumbnailUpdateClientLayoutsHandler : INotificationHandler<ThumbnailUpdateClientsLayouts>
	{
		private readonly IThumbnailManager _manager;

		public ThumbnailUpdateClientLayoutsHandler(IThumbnailManager manager)
		{
			this._manager = manager;
		}

		public Task Handle(ThumbnailUpdateClientsLayouts notification, CancellationToken cancellationToken)
		{
			this._manager.UpdateClientLayouts();
			return Task.CompletedTask;
		}
	}
}