using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Requests;
using Api.Responses;
using Refit;

namespace Api
{
	//[Headers("Authorization: Bearer")]
	public interface IProjectApi
	{
		[Get("/api/projects")]
		Task<ApiResponse<IEnumerable<Project>>> GetAll();

		[Get("/api/projects/{id}")]
		Task<ApiResponse<Project>> Get(Guid id);

		[Post("/api/projects")]
		Task<ApiResponse<Project>> Create([Body] CreateProject createProject);

		[Put("/api/projects/{id}")]
		Task<ApiResponse<Project>> Update(Guid id, [Body] UpdateProject updateProject);

		[Delete("/api/projects/{id}")]
		Task<ApiResponse<bool>> Delete(Guid id);
	}
}
