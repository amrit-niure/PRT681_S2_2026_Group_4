import type { NextConfig } from "next";

const apiUrl = process.env.API_URL ?? "http://localhost:5280";

const nextConfig: NextConfig = {
  // `next dev` would otherwise write AGENTS.md / CLAUDE.md into the project on every start.
  agentRules: false,

  // The browser talks to same-origin /backend/*, which Next forwards to the .NET API.
  // No CORS setup is needed and it keeps working when Aspire assigns the API a random port.
  async rewrites() {
    return [{ source: "/backend/:path*", destination: `${apiUrl}/api/:path*` }];
  },
};

export default nextConfig;
