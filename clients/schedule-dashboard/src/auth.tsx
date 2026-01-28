import React from "react";
import apiconfig from "./apiconfig";
import { Navigate, useLocation, useNavigate } from "react-router";

/**
 * 登录服务逻辑
 */
const loginService = {
  isAuthenticated: false,
  signin(callback: VoidFunction) {
    loginService.isAuthenticated = true;
    setTimeout(callback, 100);
  },
  signout(callback: VoidFunction) {
    loginService.isAuthenticated = false;
    setTimeout(callback, 100);
  },
};

interface AuthContextType {
  user: any;
  signin: (user: string, callback: VoidFunction) => void;
  signout: (callback: VoidFunction) => void;
}

let AuthContext = React.createContext<AuthContextType>(null!);

function AuthProvider({ children }: { children: React.ReactNode }) {
  const initialUser = () => {
    const storedUser = sessionStorage.getItem(apiconfig.loginConfig.sessionKey);
    return storedUser ? JSON.parse(storedUser) : null;
  };

  const [user, setUser] = React.useState<any>(initialUser());

  let signin = (newUser: string, callback: VoidFunction) => {
    return loginService.signin(() => {
      setUser(newUser);
      sessionStorage.setItem(
        apiconfig.loginConfig.sessionKey,
        JSON.stringify(newUser)
      );
      callback();
    });
  };

  let signout = (callback: VoidFunction) => {
    return loginService.signout(() => {
      setUser(null);
      sessionStorage.removeItem(apiconfig.loginConfig.sessionKey);
      callback();
    });
  };

  let value = { user, signin, signout };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

function AuthStatus() {
  let auth = useAuth();
  let navigate = useNavigate();

  if (!auth.user) {
    return <p>You are not logged in.</p>;
  }

  return (
    <p>
      Welcome {auth.user}!{" "}
      <button
        onClick={() => {
          auth.signout(() => navigate("/"));
        }}
      >
        Sign out
      </button>
    </p>
  );
}

function useAuth() {
  return React.useContext(AuthContext);
}

function RequireAuth({ children }: { children: JSX.Element }) {
  let auth = useAuth();
  let location = useLocation();

  if (!auth.user) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return children;
}

export { loginService, useAuth, AuthProvider, RequireAuth, AuthStatus };
