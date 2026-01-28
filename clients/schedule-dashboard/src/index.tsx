import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import apiconfig from "./apiconfig";
import "./index.css";
import Login from "./login";
import { AuthProvider, RequireAuth } from "./auth";
import { BrowserRouter, Route, Routes } from "react-router";

const root = ReactDOM.createRoot(
  document.getElementById("root") as HTMLElement
);
root.render(
  <React.StrictMode>
    <BrowserRouter basename={apiconfig.requestPath}>
      <AuthProvider>
        <Routes>
          <Route
            index
            element={
              <RequireAuth>
                <App />
              </RequireAuth>
            }
          />
          <Route path="/login" element={<Login />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>
);
