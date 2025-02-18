{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell {
  # Set the name of the environment (optional)
  name = "mdbook-web-dev";

  # Specify the packages you need
  buildInputs = [
    # mdBook for documentation
    pkgs.mdbook

    # Web development tools
    pkgs.nodejs # Node.js for JavaScript runtime
    pkgs.yarn  # Yarn for package management
    pkgs.nodePackages.typescript # TypeScript compiler
    pkgs.nodePackages.prettier # Prettier for code formatting

    # Optional: Additional web development tools
    pkgs.git # Version control
    pkgs.curl # For making HTTP requests
    pkgs.jq # For JSON processing
    pkgs.python3 # Python for scripting or backend development
    pkgs.python3Packages.pip # Python package manager
  ];

  # Environment variables (optional)
  shellHook = ''
    echo "Welcome to the mdBook and web development environment!"
    echo "Available tools:"
    echo "mdbook: $(mdbook --version)"
    echo "node: $(node --version)"
    echo "yarn: $(yarn --version)"
    echo "typescript: $(tsc --version)"
    echo "prettier: $(prettier --version)"
  '';
}
