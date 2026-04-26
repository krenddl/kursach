import { createContext, useContext, useEffect, useMemo, useState } from "react";

const AuthContext = createContext(null);

function parseJwt(token) {
  try {
    const base64 = token
      .split(".")[1]
      .replace(/-/g, "+")
      .replace(/_/g, "/");
    const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), "=");
    const json = JSON.parse(atob(padded));
    return json;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }) {
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [user, setUser] = useState(null);

  useEffect(() => {
    if (!token) {
      setUser(null);
      return;
    }

    const payload = parseJwt(token);
    if (!payload) {
      logout();
      return;
    }

    setUser({
      id:
        payload.nameid ||
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
      email: payload.email,
      role:
        payload.role ||
        payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
    });
  }, [token]);

  const login = (jwt) => {
    localStorage.setItem("token", jwt);
    setToken(jwt);
  };

  const logout = () => {
    localStorage.removeItem("token");
    setToken(null);
    setUser(null);
  };

  const value = useMemo(
    () => ({
      token,
      user,
      isAuthenticated: !!token,
      login,
      logout,
    }),
    [token, user]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}
