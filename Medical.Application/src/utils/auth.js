const TOKEN_KEY = 'medical-admin-token';

export const setToken = (token) => {
  window.localStorage.setItem(TOKEN_KEY, token);
};

export const getToken = () => window.localStorage.getItem(TOKEN_KEY);

export const clearToken = () => window.localStorage.removeItem(TOKEN_KEY);

export const isAuthenticated = () => Boolean(getToken());


