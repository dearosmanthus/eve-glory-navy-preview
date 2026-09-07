using System.Threading;
using System.Threading.Tasks;
using EveOPreview.Mediator.Messages;
using EveOPreview.Services;
using MediatR;

namespace EveOPreview.Mediator.Handlers.Thumbnails
{
	sealed class ThumbnailApplyAllClientLayoutsHandler : INotificationHandler<ThumbnailApplyAllClientsLayouts>
	{
		private readonly IThumbnailManager _manager;

		public ThumbnailApplyAllClientLayoutsHandler(IThumbnailManager manager)
		{
			this._manager = manager;
		}

		public Task Handle(ThumbnailApplyAllClientsLayouts notification, CancellationToken cancellationToken)
		{
			this._manager.ApplyAllClientLayouts();
			return Task.CompletedTask;
		}
	}
}