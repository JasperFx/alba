COMPILE_TARGET = ENV['config'].nil? ? "debug" : ENV['config']
RESULTS_DIR = "results"
BUILD_VERSION = '4.0.0'

tc_build_number = ENV["BUILD_NUMBER"]
build_revision = tc_build_number || Time.new.strftime('5%H%M')
build_number = "#{BUILD_VERSION}.#{build_revision}"
BUILD_NUMBER = build_number

task :ci => [:version, :default, :pack]

task :default => [:test]

desc "Prepares the working directory for a new build"
task :clean do
	#TODO: do any other tasks required to clean/prepare the working directory
	FileUtils.rm_rf RESULTS_DIR
	FileUtils.rm_rf 'artifacts'

end

desc "Update the version information for the build"
task :version do
  asm_version = build_number

  begin
    commit = `git log -1 --pretty=format:%H`
  rescue
    commit = "git unavailable"
  end
  puts "##teamcity[buildNumber '#{build_number}']" unless tc_build_number.nil?
  puts "Version: #{build_number}" if tc_build_number.nil?

  options = {
	:description => 'Grab bag of generic utilities and extension methods for .Net development',
	:product_name => 'Baseline',
	:copyright => 'Copyright 2015 Jeremy D. Miller et al. All rights reserved.',
	:trademark => commit,
	:version => asm_version,
	:file_version => build_number,
	:informational_version => asm_version

  }
end


desc 'Compile the code'
task :compile => [:clean] do
	sh "dotnet restore src/Alba.sln"
end

desc 'Run the unit tests'
task :test => [:compile] do
	Dir.mkdir RESULTS_DIR

	sh "dotnet test src/Alba.Testing/Alba.Testing.csproj"
end

desc "Pack up the nupkg file"
task :pack => [:compile] do
	sh "dotnet pack src/Alba/Alba.csproj -o ./artifacts --configuration Release"
end

# TODO -- redo these tasks
desc "Launches VS to the Alba solution file"
task :sln do
	sh "start src/Alba.sln"
end


"Launches the documentation project in editable mode"
task :docs do
	sh "dotnet restore"
	sh "dotnet stdocs run -v #{BUILD_VERSION}"
end

"Exports the documentation to jasperfx.github.io - requires Git access to that repo though!"
task :publish do
	FileUtils.remove_dir('doc-target') if Dir.exists?('doc-target')

	if !Dir.exists? 'doc-target'
		Dir.mkdir 'doc-target'
		sh "git clone -b gh-pages https://github.com/jasperfx/alba.git doc-target"
	else
		Dir.chdir "doc-target" do
			sh "git checkout --force"
			sh "git clean -xfd"
			sh "git pull origin master"
		end
	end

	sh "dotnet restore"
	sh "npm run docs-build"


	Dir.glob('/docs/.vitepress/dist/**/*.*').each do |file|
	  dir, filename = File.dirname(file), File.basename(file)
	  dest = File.join(@target_dir, dir)
	  FileUtils.mkdir_p(dest)
	  FileUtils.copy_file(file, File.join(dest,filename))
	end

	Dir.chdir "doc-target" do
		sh "git add --all"
		sh "git commit -a -m \"Documentation Update for #{BUILD_VERSION}\" --allow-empty"
		sh "git push origin gh-pages"
	end




end
