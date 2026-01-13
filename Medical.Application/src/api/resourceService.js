import httpClient from './httpClient.js';

export const resourceService = {
  list: (basePath, params) => httpClient.get(basePath, { params }),
  get: (url) => httpClient.get(url),
  getById: (basePath, id) => httpClient.get(`${basePath}/${id}`),
  create: (basePath, payload) => httpClient.post(basePath, payload),
  update: (basePath, id, payload) => httpClient.put(`${basePath}/${id}`, payload),
  remove: (basePath, id) => httpClient.delete(`${basePath}/${id}`)
};

export default resourceService;

