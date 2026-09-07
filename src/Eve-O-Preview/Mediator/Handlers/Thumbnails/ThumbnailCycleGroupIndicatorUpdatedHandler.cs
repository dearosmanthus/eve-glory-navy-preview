using System.Threading;
using System.Threading.Tasks;
using EveOPreview.Mediator.Messages;
using EveOPreview.Services;
using MediatR;

namespace EveOPreview.Mediator.Handlers.Thumbnails
{
	sealed class ThumbnailCycleGroupIndicatorUpdatedHandler : INotificationHandler<ThumbnailCycleGroupIndicatorUpdated>
	{
		private readonly IThumbnailManager _manager;

		public ThumbnailCycleGroupIndicatorUpdatedHandler(IThumbnailManager manager)
		{
			this._manager = manager;
		}

		public Task Handle(ThumbnailCycleGroupIndicatorUpdated notification, CancellationToken cancellationToken)
		{
			this._manager.UpdateCycleGroupIndicator();

			return Task.CompletedTask;
		}
	}
}