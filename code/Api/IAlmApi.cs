using System;
using System.Threading.Tasks;
using Api.Responses;
using Refit;

namespace Api
{
	public interface IAlmApi
	{
		[Get("/alm/projects/{id}")]
		Task<ApiResponse<Project>> GetProject(Guid id);
	}
}
