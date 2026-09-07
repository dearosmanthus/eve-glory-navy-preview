using System.Threading;
using System.Threading.Tasks;
using EveOPreview.Mediator.Messages;
using EveOPreview.Services;
using MediatR;

namespace EveOPreview.Mediator.Handlers.Configuration
{
	sealed class HotkeysConfigurationUpdatedHandler : INotificationHandler<HotkeysConfigurationUpdated>
	{
		private readonly IThumbnailManager _manager;

		public HotkeysConfigurationUpdatedHandler(IThumbnailManager manager)
		{
			this._manager = manager;
		}

		public Task Handle(HotkeysConfigurationUpdated notification, CancellationToken cancellationToken)
		{
			this._manager.ReloadCycleClientHotkeys();
			return Task.CompletedTask;
		}
	}
}
